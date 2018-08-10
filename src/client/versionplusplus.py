#! /usr/bin/python

import os, sys, json, datetime


def update(jobj):
    ver = jobj["version"]
    versplits = ver.split('.')
    newver = int(versplits[1]) +1 #we increment 0.2.0, the second split item. Developer is in charge of updating the first number.
    d = datetime.datetime.now()    
    ds = (d-datetime.datetime(1970,1,1)).total_seconds() * 1000
    newdate = int(ds)
    versplits[1] = str(newver)
    newver = ".".join(versplits)
    jobj["version"] = newver
    jobj["built"] = newdate
    return

def main():
    try:
        fpath = os.path.join(os.curdir, "public", "data", "version.json")
        if os.path.exists(fpath):
            j = None
            with open(fpath, 'r') as f:
                j = json.load(f)
            update(j)
            with open(fpath, 'w') as f:
                json.dump(j, f)
        return                
    except Exception as e:
        print(e)
        print("An error ocurred and version number could not be updated.")
    return

if __name__ == "__main__":
    main()
     