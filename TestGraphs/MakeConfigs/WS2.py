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

# FieldNBalance is a program that estimates the N balance and provides N fertilizer recommendations for cultivated crops.
# Author: Hamish Brown.
# Copyright (c) 2024 The New Zealand Institute for Plant and Food Research Limited

import os
import pandas as pd

Sites = {
 1: 'Wilcox',
 2: 'Jivans',
 3: 'Bhana',
 4: 'Balles',
 5: 'Pescini',
 6: 'Woodhaven',
 7: 'Brownrigg',
 8: 'Lovett',
 9: 'Oakley'
}

try: 
    if os.environ["GITHUB_WORKSPACE"] != None:
        root = os.environ["GITHUB_WORKSPACE"]
        path = os.path.join(root,"TestComponents", "TestSets", "WS2")
except:
    rootfrags = os.path.abspath('WS2.py').split("\\")
    root = ""
    for d in rootfrags:
        if d == "FieldNBalance":
            break
        else:
            root += d + "\\"
    path = os.path.join(root,"FieldNBalance","TestComponents", "TestSets", "WS2")

Configs = pd.read_excel(os.path.join(path, "FieldConfigs.xlsx"),sheet_name=Sites[1],nrows=45,usecols=lambda x: 'Unnamed' not in x,keep_default_na=False)
Configs.set_index('Name',inplace=True)
for s in range(2,10):
    sites = pd.read_excel(os.path.join(path, "FieldConfigs.xlsx"),sheet_name=Sites[s],nrows=45,usecols=lambda x: 'Unnamed' not in x,keep_default_na=False)
    sites.set_index('Name',inplace=True)
    Configs = pd.concat([Configs,sites],axis=1)

CSConfigs = Configs.transpose()
CSConfigs.to_csv(os.path.join(path, "FieldConfigs.csv"),header=True)

Configs.to_pickle(os.path.join(path, "FieldConfigs.pkl"))
