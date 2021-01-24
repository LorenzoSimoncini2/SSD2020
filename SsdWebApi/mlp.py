import numpy as np, pandas as pd
import matplotlib.pyplot as plt
import os, sys, io, base64, math
from statsmodels.graphics.tsaplots import plot_acf, plot_pacf
from keras.models import Sequential
from keras.layers import Dense 
import keras.optimizers as opts 
import tensorflow.python.keras.backend as K
import joblib
import tensorflow as tf
from sklearn import model_selection as ms
def print_figure(fig):
	"""
	Converts a figure (as created e.g. with matplotlib or seaborn) to a png image and this 
	png subsequently to a base64-string, then prints the resulting string to the console.
	"""
	buf = io.BytesIO()
	fig.savefig(buf, format='png')
	print(base64.b64encode(buf.getbuffer()))


# from series of values to windows matrix
def compute_windows(nparray, npast=1):
        dataX, dataY = [], [] # window and value
        for i in range(len(nparray)-npast-1):
                a = nparray[i:(i+npast), 0]
                dataX.append(a)
                dataY.append(nparray[i + npast, 0])
        return np.array(dataX), np.array(dataY)




if __name__ == "__main__":
   # change working directory to script path
   abspath = os.path.abspath(__file__)
   dname = os.path.dirname(abspath)
   os.chdir(dname)
   seed_value = 56#np.random.seed(550) # for reproducibility
   dffile = sys.argv[1]
   #df = pd.read_csv("../"+dffile, header=0)
   df = pd.read_csv(dffile, header=0)
   datasetValue = df[dffile[:-4]].values.reshape(-1,1) # time series values
   datasetValue = datasetValue.astype('float32') 
   previsionwindow = 120
   #120gg : npast=30 , nhidden =15
   #480gg : npast=30 , hidden = 20
   datasetwithoutForecast =  datasetValue[:]#(len(datasetValue)-previsionwindow)]
   logdata =np.log(datasetwithoutForecast)
   datasetForecast = datasetValue[-previsionwindow:]
   logForecast=np.log(datasetForecast)
   # train - test sets
   cutpoint = int(len(logdata)-previsionwindow)#int(0.9*len(logdata)) # 70% train, 30% test
   train, test = logdata[:cutpoint], logdata[cutpoint:]
   #reconstruct
   #reconstruct = np.exp(np.r_[train,test]) # simple reconstruction
   
   tf.random.set_seed(seed_value)
   # for later versions: 
   # tf.compat.v1.set_random_seed(seed_value)
    
   # 5. Configure a new global `tensorflow` session
   
   session_conf = tf.compat.v1.ConfigProto(intra_op_parallelism_threads=1, inter_op_parallelism_threads=1)
   sess = tf.compat.v1.Session(graph=tf.compat.v1.get_default_graph(), config=session_conf)
   K.set_session(sess)
   
   
   
   # sliding window matrices (npast = window width); dim = n - npast - 1
   npast =90
   trainX, trainY = compute_windows(train, npast)
   testX, testY = compute_windows(test, npast) # should get also the last npred of train
   
   # Multilayer Perceptron model
   model = Sequential()
   n_hidden =npast
   n_output = 1
   model.add(Dense(n_hidden, input_dim=npast, activation='relu')) 
   model.add(Dense(n_hidden+1, input_dim=npast, activation='relu')) 
   model.add(Dense(n_output)) # output neurons
   opt = opts.Adam(learning_rate=0.00003)
   model.compile(loss='mean_squared_error', optimizer=opt)
   batch = len(trainX)
   #print(K.eval(model.optimizer.lr))
   bs = 132#int(len(trainX) / 4)#len(trainX)
   history = model.fit(trainX, trainY, epochs=400, batch_size=bs  , verbose=2 ) # batch_size len(trainX)    
 
   #model = tf.keras.models.load_model('model_ustreasury.h5')
 
   
   plt.subplot(2, 1, 1)
   loss = model.history.history['loss']  
   plt.plot(range(len(loss)),loss);
   plt.ylabel('loss')
   plt.show()
   # Model performance
   trainScore = model.evaluate(trainX, trainY, verbose=0)
   print('Score on train: MSE = {0:0.5f} '.format(trainScore))
   testScore = model.evaluate(testX, testY, verbose=0)
   print('Score on test: MSE = {0:0.5f} '.format(testScore))
   
   trainPredict = model.predict(trainX) # predictions
   testForecast = model.predict(testX) # forecast

   foretestX, a = compute_windows(trainPredict, npast)
   forecastresult = [0]*previsionwindow
   for i in range(previsionwindow):
           t= foretestX[-npast:,:]
           forecastvalue = model.predict(t)
           forecastresult[i]= forecastvalue[-1:,0][0]
           a = np.append(t,forecastvalue.T,axis=0)
           foretestX = a
   
   plt.subplot(2, 1, 2)
   #plt.plot(logdata)
   plt.plot(np.log(datasetValue))
   plt.plot(np.concatenate((np.full(npast,np.nan),trainPredict[:,0])))
   plt.plot(np.concatenate((np.full(len(train)-1+npast,np.nan), testForecast[:,0])))
   plt.plot(np.concatenate((np.full(len(train)-1,np.nan),forecastresult)))
   #plt.plot(np.concatenate((np.full(len(datasetwithoutForecast),np.nan),forecastresult)))
   plt.subplot(2, 2, 1)

   plt.plot(np.exp(logForecast))
   plt.plot(np.exp(forecastresult))
   plt.show()
   
   model.save("model_name.h5")
   print("Saved model to disk")
