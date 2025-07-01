using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class JudgeNotes : MonoBehaviour
{

    void Update()
    {

        if (GetComponent<OnPlaying>().isStart)
        {
            HashSet<float> currentTouch = new HashSet<float>();

            foreach (Touch finger in Input.touches)
            {
                Ray touchray = Camera.main.ScreenPointToRay(finger.position);
                RaycastHit touchhit;

                if (Physics.Raycast(touchray, out touchhit))
                {
                    bool FirstTap = false;
                    RaycastHit[] allhits = Physics.RaycastAll(touchray);
                    var sortedHits = allhits
                        .Select(hit => new
                        {
                            Hit = hit,
                            Notes = hit.transform.GetComponent<JudgeCollider>().ParentNote.GetComponentInParent<NoteEntity>(),
                            DistanceToCenter = (hit.point - hit.transform.position).magnitude
                        })
                        .Where(x => x.Notes != null) // ??????????????NotesHighLight????
                        .OrderBy(x => x.Notes.HitBeat) // ????????hittime????
                        .ThenBy(x =>
                            // 第二优先：标签优先级，Tap 排最前，Drag 次之，Hold 最后
                            x.Notes.CompareTag("Tap")  ? 0 :
                            x.Notes.CompareTag("Drag") ? 1 :
                            x.Notes.CompareTag("Hold") ? 2 : 3
                        )
                        .ThenBy(x => x.DistanceToCenter) // ????hittime??????????????????????????????
                        .Select(x => x.Hit) // ????????????RaycastHit????
                        .ToArray();
                    foreach (RaycastHit allhit in sortedHits)
                    {
                        GameObject note = allhit.transform.GetComponent<JudgeCollider>().ParentNote;

                        if (finger.phase == TouchPhase.Began)
                        {
                            if (FirstTap == false && note.CompareTag("Tap"))
                            {
                                Tap tap = note.GetComponent<Tap>();
                                if (tap.Hitted == false)
                                {
                                    tap.Hit();
                                    FirstTap = true;
                                    continue;
                                }
                            }
                            /*
                            else
                            {
                                FirstTap = true;
                            }
                            */
                        }
                        if (note.CompareTag("Drag"))
                        {
                            Drag drag = note.GetComponent<Drag>();
                            drag.Tagged = true;
                        }
                        if (note.CompareTag("Hold"))
                        {
                            Hold hold = note.GetComponent<Hold>();
                            hold.TagElement(allhit.collider);
                        }

                    }

                }

            }
            if (Input.mousePresent)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    GameObject note = hit.transform.GetComponent<JudgeCollider>().ParentNote;
                    RaycastHit[] allhits = Physics.RaycastAll(ray);
                    foreach (RaycastHit allhit in allhits)
                    {
                        if (allhit.transform.tag == "Drag")
                        {
                            allhit.transform.GetComponent<JudgeCollider>().ParentNote.GetComponentInParent<Drag>().Tagged = true;
                        }

                    }
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (note != null)
                        {
                            if (note.tag == "Drag")
                            {
                                Drag drag = note.GetComponent<Drag>();
                                drag.Tagged = true;
                            }
                            if (note.tag == "Hold")
                            {
                                Hold hold = note.GetComponent<Hold>();
                                hold.TagElement(hit.collider);

                            }
                            if (note.tag == "Tap")
                            {
                                Tap tap = note.GetComponent<Tap>();
                                tap.Hit();
                            }
                        }
                    }
                    if (Input.GetMouseButton(0))
                    {
                        if (note.tag == "Drag")
                        {
                            Drag drag = note.GetComponent<Drag>();
                            drag.Tagged = true;
                        }
                        if (note.tag == "Hold")
                        {
                            Hold hold = note.GetComponent<Hold>();
                            hold.TagElement(hit.collider);

                        }
                    }
                }
            }
        }
    }
}
