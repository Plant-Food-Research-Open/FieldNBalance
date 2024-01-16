# ---
# jupyter:
#   jupytext:
#     formats: ipynb,py:light
#     text_representation:
#       extension: .py
#       format_name: light
#       format_version: '1.5'
#       jupytext_version: 1.15.0
#   kernelspec:
#     display_name: Python 3 (ipykernel)
#     language: python
#     name: python3
# ---

# +
#from io import BytesIO
import os
import pandas as pd
import matplotlib.pyplot as plt
import datetime as dt
import matplotlib.dates as mdates

CBcolors = {
    'blue':    '#377eb8', 
    'orange':  '#ff7f00',
    'green':   '#4daf4a',
    'pink':    '#f781bf',
    'brown':   '#a65628',
    'purple':  '#984ea3',
    'gray':    '#999999',
    'red':     '#e41a1c',
    'yellow':  '#dede00'
} 
# -

inPath = os.getcwd().split("\\SVSModelBuildDeploy\\")[0]+"\\SVSModelBuildDeploy\\TestComponents\\TestSets\\WS2"
outPath = os.getcwd().split("\\SVSModelBuildDeploy\\")[0]+"\\SVSModelBuildDeploy\\TestGraphs\\Outputs\\"

Configs = pd.read_pickle(inPath+"\\FieldConfigs.pkl")

observedCrop = pd.read_csv(inPath + "\\CropData.csv",index_col=0)
observedCrop.sort_index(axis=0,inplace=True)
observedCrop['Date'] = pd.to_datetime(observedCrop['Date'],dayfirst=True)

observedSoil = pd.read_csv(inPath + "\\SoilData.csv",index_col=0)
observedSoil.sort_index(axis=0,inplace=True)
observedSoil['Date'] = pd.to_datetime(observedSoil['Date'],dayfirst=True)
observedSoil['SoilMineralN'] = observedSoil.loc[:,['SoilN0_15', 'SoilN15_30', 'SoilN30_60', 'SoilN60_90']].sum(axis=1)

testFiles = []
tests = []
for file in os.listdir(inPath+"\\Outputs"):
    if file.endswith('.csv'):
        testFiles.append(file)
        tests.append(file.replace(".csv",""))

# +
Alltests =[]
for t in testFiles[:]:  
    testframe = pd.read_csv(inPath+"\\Outputs\\"+t,index_col=0,dayfirst=True,date_format='%d/%m/%Y %H:%M:%S %p')  
    Alltests.append(testframe)   

AllData = pd.concat(Alltests,axis=1,keys=tests)
AllData.sort_index(axis=0,inplace=True)
AllData.index = pd.to_datetime(AllData.index)

# +
colors = ['orange','green']
Graph = plt.figure(figsize=(10,10))
pos = 1
row_num=len(tests)

for t in tests:
    site = ''
    for c in t:     
        if c in ['0','1','2','3','4','5','6','7','8','9']: site += c
    site = int(site)
    
    dates = AllData.loc[Configs.loc["PriorHarvestDate",t]:Configs.loc["CurrentHarvestDate",t],(t,'CropN')].index
    c = 0    
    for v in ['SoilMineralN','CropN']:
        ax = Graph.add_subplot(row_num,2,pos)
        Data = AllData.loc[dates,(t,v)]
        plt.plot(Data,color=CBcolors[colors[c]],label=v)
        
        if v == 'CropN':
            sData = observedCrop.loc[site,:]
        if v == 'SoilMineralN':
            sData = observedSoil.loc[site,:]
        dFilter = [dates.min() <= sData['Date'].iloc[x] <= dates.max() for x in range(len(sData['Date']))]
        plt.plot(sData.loc[dFilter,'Date'],sData.loc[dFilter,v],'o',color=CBcolors[colors[c]])
                        
        plt.title(t)
        plt.xticks(rotation=60)
        ax.xaxis.set_major_formatter(mdates.DateFormatter('%#d-%b'))
        plt.legend()
        Graph.tight_layout(pad=1.5)
        pos+=1
        c+=1

plt.savefig(outPath+'\\TimeCourse.png')
# +
# colors = ['orange','green']
# Graph = plt.figure(figsize=(10,10))
# pos = 1
# row_num=len(test_names)

# for t in test_names:
#     dates = AllData.loc[Configs.loc["PriorHarvestDate",t]:Configs.loc["CurrentHarvestDate",t],(t,'CropN')].index
#     c = 0    
#     for v in ['ResidueN','SoilOMN']:
#         color = 'b'
#         ax = Graph.add_subplot(row_num,2,pos)
#         Data = AllData.loc[dates,(t,v)].cumsum()
#         plt.plot(Data,color=CBcolors[colors[c]],label=v)
#         plt.title(t)
#         plt.xticks(rotation=60)
#         ax.xaxis.set_major_formatter(mdates.DateFormatter('%#d-%b'))
#         plt.legend()
#         Graph.tight_layout(pad=1.5)
#         pos+=1
#         c+=1
# +
# colors = ['orange','green']
# Graph = plt.figure(figsize=(10,10))
# pos = 1
# row_num=len(test_names)

# for t in test_names:
#     dates = AllData.loc[Configs.loc["PriorHarvestDate",t]:Configs.loc["CurrentHarvestDate",t],(t,'CropN')].index
#     c = 0    
#     for v in ['Drainage', 'Irrigation']:
#         color = 'b'
#         ax = Graph.add_subplot(row_num,2,pos)
#         Data = AllData.loc[dates,(t,v)].cumsum()
#         plt.plot(Data,color=CBcolors[colors[c]],label=v)
#         plt.title(t)
#         plt.xticks(rotation=60)
#         ax.xaxis.set_major_formatter(mdates.DateFormatter('%#d-%b'))
#         plt.legend()
#         Graph.tight_layout(pad=1.5)
#         pos+=1
#         c+=1
# +
# colors = ['orange','green']
# Graph = plt.figure(figsize=(10,10))
# pos = 1
# row_num=len(test_names)

# for t in test_names:
#     dates = AllData.loc[Configs.loc["PriorHarvestDate",t]:Configs.loc["CurrentHarvestDate",t],(t,'CropN')].index
#     c = 0    
#     for v in ['Green cover', 'RSWC']:
#         color = 'b'
#         ax = Graph.add_subplot(row_num,2,pos)
#         Data = AllData.loc[dates,(t,v)]
#         plt.plot(Data,color=CBcolors[colors[c]],label=v)
#         plt.title(t)
#         plt.xticks(rotation=60)
#         ax.xaxis.set_major_formatter(mdates.DateFormatter('%#d-%b'))
#         plt.legend()
#         Graph.tight_layout(pad=1.5)
#         pos+=1
#         c+=1
