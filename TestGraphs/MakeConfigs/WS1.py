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

import os
import pandas as pd

Sites = {
 1: 'LincolnRot1',
 2: 'LincolnRot2',
 3: 'HawksBayRot3',
 4: 'HawksBayRot4'
}

try: 
    if os.environ["GITHUB_WORKSPACE"] != None:
        root = os.environ["GITHUB_WORKSPACE"]
        path = os.path.join(root,"TestComponents", "TestSets", "WS1")
except:
    rootfrags = os.path.abspath('WS1.py').split("\\")
    root = ""
    for d in rootfrags:
        if d == "FieldNBalance":
            break
        else:
            root += d + "\\"
    path = os.path.join(root,"FieldNBalance","TestComponents", "TestSets", "WS1")

Configs = pd.read_excel(os.path.join(path, "FieldConfigs.xlsx"),sheet_name=Sites[1],nrows=45,usecols=lambda x: 'Unnamed' not in x,keep_default_na=False)
Configs.set_index('Name',inplace=True)
for s in range(1,5):
    sites = pd.read_excel(os.path.join(path, "FieldConfigs.xlsx"),sheet_name=Sites[s],nrows=45,usecols=lambda x: 'Unnamed' not in x,keep_default_na=False)
    sites.set_index('Name',inplace=True)
    if s != 1:
        Configs = pd.concat([Configs,sites],axis=1)
    #set the other N treatments
    nlast = "_N1_"
    for n in ["_N2_","_N3_","_N4_"]:
        sites.columns = [sites.columns[x].replace(nlast,n) for x in range(sites.columns.size)]
        Configs = pd.concat([Configs,sites],axis=1)
        nlast = n

CSConfigs = Configs.transpose()
CSConfigs.to_csv(os.path.join(path, "FieldConfigs.csv"),header=True)

Configs.to_pickle(os.path.join(path, "FieldConfigs.pkl"))
