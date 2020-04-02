using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseData : ScriptableObject
{
	public const string dataDirectory = "/9.ResourcesData/Resources/Data/";

    public string[] names = null;

    protected static string NAME = "name";

    public BaseData() { }

    public int GetDataCount()
    {
        int retVal = 0;
        if (this.names != null)
        {
            retVal = this.names.Length;
        }

        return retVal;
    }

    public string[] GetNameList(bool showID, string filterWord = "")
    {
        string[] retList = new string[0];

        if (this.names != null)
        {
            retList = new string[this.names.Length];

            for (int i = 0; i < this.names.Length; i++)
            {
                if (filterWord != "")
                {
                    if (this.names[i].ToLower().Contains(filterWord) == false)
                    {
                        continue;
                    }
                }

                if (showID == true)
                {
                    retList[i] = i.ToString() + ":" + this.names[i];
                }
                else
                {
                    retList[i] = this.names[i];
                }
            }
        }

        return retList;
    }

    public virtual void RemoveData(int index)
    {
        
    }

    public virtual void Copy(int index)
    {
        
    }


}
