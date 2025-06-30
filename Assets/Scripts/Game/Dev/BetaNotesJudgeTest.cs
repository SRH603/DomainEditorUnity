using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BetaNotesJudgeTest : MonoBehaviour
{
    public Toggle BetaControl;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (BetaControl.isOn)
        {
            if (this.GetComponent<CapsuleCollider>().enabled == true)
            {
                this.GetComponent<MeshRenderer>().enabled = true;
            }
            else
            {
                this.GetComponent<MeshRenderer>().enabled = false;
            }
        }
        else
        {
            this.GetComponent<MeshRenderer>().enabled = false;
        }
        
    }
}
