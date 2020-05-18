using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//데이터의 근간이 되는 클래스.
//공통적인 데이터를 가지고 있다. 지금은 이름이랑 결로만 가지고 있다.
//데이터 갯수와 이름 리스트를 얻을 수 있다.
public class BaseData : ScriptableObject
{
	public const string dataDirectory = "/9.ResourcesData/Resources/Data/";

    public string[] names = null;

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

    public virtual int AddData(string newName)
    {
        return GetDataCount();
    }

    public virtual void RemoveData(int index)
    {
        
    }

    public virtual void Copy(int index)
    {
        
    }


}
