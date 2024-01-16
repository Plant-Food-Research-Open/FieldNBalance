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

path = os.getcwd().split("\\SVSModelBuildDeploy\\")[0]+"\\SVSModelBuildDeploy\\TestComponents\\TestSets\\Location"

Configs = pd.read_excel(path+"\\FieldConfigs.xlsx",nrows=48,usecols=lambda x: 'Unnamed' not in x)

Configs.set_index('Name',inplace=True)

CSConfigs = Configs.transpose()
CSConfigs.to_csv(path+"\\FieldConfigs.csv",header=True)

Configs.to_pickle(path+"\\FieldConfigs.pkl")
