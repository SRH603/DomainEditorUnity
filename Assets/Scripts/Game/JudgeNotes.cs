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
                    //currentTouch.Add(hit.point.x); // ????????????????????
                    //touch.Add(hit.point.x); // ????????????????
                    //NotesHighLight notesHighLight = touchhit.transform.GetComponent<JudgeCollider>().ParentNote.GetComponentInParent<NotesHighLight>();
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
                        .ThenBy(x => x.DistanceToCenter) // ????hittime??????????????????????????????
                        .Select(x => x.Hit) // ????????????RaycastHit????
                        .ToArray();
                    /*
                    foreach (RaycastHit allhit in sortedHits)
                    {
                        NotesHighLight allnotesHighLight = allhit.transform.GetComponent<JudgeCollider>().ParentNote.GetComponentInParent<NotesHighLight>();

                        if (finger.phase == TouchPhase.Began)
                        {
                            if (FirstTap == false && allnotesHighLight.notetype == 0)
                            {
                                if (allnotesHighLight.Hitted == false)
                                {
                                    allnotesHighLight.Hit();
                                    FirstTap = true;
                                    continue;
                                }
                            }
                            else
                            {
                                FirstTap = true;
                            }
                        }
                        if (allnotesHighLight != null && allnotesHighLight.notetype == 1)
                        {
                            allnotesHighLight.TaggedDrag = true;
                        }
                        if (allnotesHighLight != null && allnotesHighLight.notetype == 2)
                        {
                            allnotesHighLight.ProcessingHold(); // ????????
                        }

                    }
                    */
                    foreach (RaycastHit allhit in sortedHits)
                    {
                        GameObject note = allhit.transform.GetComponent<JudgeCollider>().ParentNote;

                        if (finger.phase == TouchPhase.Began)
                        {
                            if (FirstTap == false && note.tag == "Tap")
                            {
                                Tap tap = note.GetComponent<Tap>();
                                if (tap.Hitted == false)
                                {
                                    tap.Hit();
                                    FirstTap = true;
                                    continue;
                                }
                            }
                            else
                            {
                                FirstTap = true;
                            }
                        }
                        if (note.tag == "Drag")
                        {
                            Drag drag = note.GetComponent<Drag>();
                            drag.Tagged = true;
                        }
                        if (note.tag == "Hold")
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
                    //currentTouch.Add(hit.point.x); // ????????????????????
                    //touch.Add(hit.point.x); // ????????????????
                    /*
                    NotesHighLight notesHighLight = hit.transform.GetComponent<JudgeCollider>().ParentNote.GetComponentInParent<NotesHighLight>();
                    RaycastHit[] allhits = Physics.RaycastAll(ray);
                    foreach (RaycastHit allhit in allhits)
                    {
                        NotesHighLight allnotesHighLight = allhit.transform.GetComponent<JudgeCollider>().ParentNote.GetComponentInParent<NotesHighLight>();
                        if (allnotesHighLight != null && allnotesHighLight.notetype == 1)
                        {
                            allnotesHighLight.TaggedDrag = true;
                        }

                    }
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (notesHighLight != null)
                        {
                            if (notesHighLight.notetype == 1)
                            {
                                notesHighLight.TaggedDrag = true;

                            }
                            if (notesHighLight.notetype == 2)
                            {
                                notesHighLight.ProcessingHold();
                            }
                            if (notesHighLight.notetype == 0)
                            {
                                //notesHighLight.Hit();
                                notesHighLight.gameObject.GetComponent<Tap>().Hit();
                            }


                        }
                    }
                    if (Input.GetMouseButton(0))
                    {
                        if (notesHighLight != null && notesHighLight.notetype == 1)
                        {
                            notesHighLight.TaggedDrag = true;

                        }
                        if (notesHighLight != null && notesHighLight.notetype == 2)
                        {
                            notesHighLight.ProcessingHold();
                        }
                    }
                    */
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
