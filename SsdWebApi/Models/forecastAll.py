# -*- coding: utf-8 -*-
"""
Created on Wed Dec 16 09:35:57 2020

@author: lorenzo.simoncini2
"""

import numpy as np, pandas as pd
import matplotlib.pyplot as plt
import os
import tensorflow.python.keras.backend as K
import tensorflow as tf
import csv

# from series of values to windows matrix
def compute_windows(nparray, npast=1):
        dataX, dataY = [], [] # window and value
        for i in range(len(nparray)-npast-1):
                a = nparray[i:(i+npast), 0]
                dataX.append(a)
                dataY.append(nparray[i + npast, 0])
        return np.array(dataX), np.array(dataY)
    
def compute_forecast(trainP, npast, forecastPeriod):
   foretestX, a = compute_windows(trainP, npast)
   forecastresult = [0]*forecastPeriod
   for i in range(forecastPeriod):
           t= foretestX[-npast:,:]
           forecastvalue = model.predict(t)
           forecastresult[i]= forecastvalue[-1:,0][0]
           a = np.append(t,forecastvalue.T,axis=0)
           foretestX = a
   return forecastresult
    
if __name__ == "__main__":
   #--preparation-- 
    
   os.environ['TF_FORCE_GPU_ALLOW_GROWTH'] = 'true'
   # change working directory to script path
   abspath = os.path.abspath(__file__)
   dname = os.path.dirname(abspath)
   os.chdir(dname)
   seed_value = 56 #random seed for reproducibility
   tf.random.set_seed(seed_value)   
   session_conf = tf.compat.v1.ConfigProto(intra_op_parallelism_threads=1, inter_op_parallelism_threads=1)
   sess = tf.compat.v1.Session(graph=tf.compat.v1.get_default_graph(), config=session_conf)
   K.set_session(sess) #sets session based on random seed
   file_indici = ["SP_500","FTSE_MIB_","GOLD_SPOT","MSCI_EM","MSCI_EURO","All_Bonds","Us_Treasury"]
   npastForIndex = [80,90,80,90,100,60,80]
   
   forecastList = []
   
   #--loop--
   for i in range(len(file_indici)):
       
       dffile = file_indici[i]+".csv"
       df = pd.read_csv(dffile, header=0)
       datasetValue = df[dffile[:-4]].values.reshape(-1,1) # time series values
       datasetValue = datasetValue.astype('float32') 
       previsionwindow = 120
       logdata =np.log(datasetValue)
       datasetForecast = datasetValue[-previsionwindow:]
       logForecast=np.log(datasetForecast)
       # train - test sets
       cutpoint = int(len(logdata)-previsionwindow)#test = dataset - previsionWindow
       train, test = logdata[:cutpoint], logdata[cutpoint:]
       
       
       # sliding window matrices (npast = window width); 
       npast =npastForIndex[i]
       trainX, trainY = compute_windows(train, npast)
       testX, testY = compute_windows(test, npast) # should get also the last npred of train
       
       model = tf.keras.models.load_model('models/'+file_indici[i]+'.h5')
       
       trainPredict = model.predict(trainX) # predictions
    
       forecastresult = compute_forecast(trainPredict, npast, previsionwindow)
       
       plt.subplot(2, 1, 2)
       plt.title(file_indici[i], loc='right')
       plt.plot(np.log(datasetValue))
       plt.plot(np.concatenate((np.full(npast-1,np.nan),trainPredict[:,0])))
       plt.plot(np.concatenate((np.full(len(train)-1,np.nan),forecastresult)))
       plt.subplot(2, 2, 1)
    
       forecastReconstructed = np.exp(logForecast)
       resultExp = np.exp(forecastresult)
       plt.plot(forecastReconstructed)
       plt.plot(resultExp)
       plt.show()
       
       forecastList.append(resultExp)
   print(forecastList)
   with open('forecast.csv', 'w', newline='') as myfile:
    for i in range(len(file_indici)):
        wr = csv.writer(myfile, quoting=csv.QUOTE_NONE)
        wr.writerow(forecastList[i])
           