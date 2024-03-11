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

rootfrags = os.abspath('WS2.ipynb').split("\\")
root = ""
for d in rootfrags:
    if d == "FieldNBalance":
        break
    else:
        root += d + "\\"
path = os.join(root,"FieldNBalance","TestComponents", "TestSets", "Location")

Configs = pd.read_excel(os.join(path, "FieldConfigs.xlsx"),nrows=48,usecols=lambda x: 'Unnamed' not in x)

Configs.set_index('Name',inplace=True)

CSConfigs = Configs.transpose()
CSConfigs.to_csv(os.join(path, "FieldConfigs.csv"),header=True)

Configs.to_pickle(os.join(path, "FieldConfigs.pkl"))
