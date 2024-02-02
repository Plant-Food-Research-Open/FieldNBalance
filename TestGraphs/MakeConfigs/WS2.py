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

from os.path import join
import pandas as pd

path = join("TestComponents", "TestSets", "WS2")
Configs = pd.read_excel(join(path, "FieldConfigs.xlsx"),nrows=48,usecols=lambda x: 'Unnamed' not in x)

Configs.set_index('Name',inplace=True)

CSConfigs = Configs.transpose()
CSConfigs.to_csv(join(path, "FieldConfigs.csv"),header=True)

Configs.to_pickle(join(path, "FieldConfigs.pkl"))
