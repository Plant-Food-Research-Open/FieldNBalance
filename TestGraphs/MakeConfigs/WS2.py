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

import os.path as os
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

rootfrags = os.abspath('WS2.ipynb').split("\\")
root = ""
for d in rootfrags:
    if d == "FieldNBalance":
        break
    else:
        root += d + "\\"
path = os.join(root,"FieldNBalance","TestComponents", "TestSets", "WS2")

Configs = pd.read_excel(os.join(path, "FieldConfigs.xlsx"),sheet_name=Sites[1],nrows=48,usecols=lambda x: 'Unnamed' not in x,keep_default_na=False)
Configs.set_index('Name',inplace=True)
for s in range(2,10):
    sites = pd.read_excel(os.join(path, "FieldConfigs.xlsx"),sheet_name=Sites[s],nrows=48,usecols=lambda x: 'Unnamed' not in x,keep_default_na=False)
    sites.set_index('Name',inplace=True)
    Configs = pd.concat([Configs,sites],axis=1)

CSConfigs = Configs.transpose()
CSConfigs.to_csv(os.join(path, "FieldConfigs.csv"),header=True)

Configs.to_pickle(os.join(path, "FieldConfigs.pkl"))
