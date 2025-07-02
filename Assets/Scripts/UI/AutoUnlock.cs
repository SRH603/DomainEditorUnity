using System.Collections.Generic;
using UnityEngine;
using static GameUtilities.Archive;

public static class AutoUnlock
{
    public static bool Evaluate(Condition condition)
    {
        bool unlock = true;
        switch (condition.type)
                {
                    case ConditionType.None:
                        break;
                    case ConditionType.OtherTrack:
                        foreach (var track in archive.tracks)
                        {
                            bool finished = false;
                            if (track.id == condition.otherTrackId)
                            {
                                bool Bool = false;
                                foreach (var chart in track.charts)
                                {
                                    if (chart.score >= condition.targetScore)
                                    {
                                        Bool = true;
                                        finished = true;
                                        break;
                                    }
                                }

                                if (!Bool)
                                {
                                    unlock = false;
                                }
                            }
                            if (finished)
                            {
                                break;
                            }
                        }

                        break;
                
                    case ConditionType.OtherChart:
                        foreach (var track in archive.tracks)
                        {
                            if (track.id == condition.otherTrackId && track.charts[condition.ratingClass].score >= condition.targetScore)
                            {
                                unlock = true;
                                break;
                            }
                            else
                            {
                                unlock = false;
                            }
                        }
                        break;

                    case ConditionType.Pack:
                        foreach (var pack in archive.packs)
                        {
                            if (pack.id == condition.otherPackId && pack.unlocked)
                            {
                                unlock = true;
                                break;
                            }
                            else
                            {
                                unlock = false;
                            }
                        }
                        break;
                    case ConditionType.Track:
                        foreach (var track in archive.tracks)
                        {
                            if (track.id == condition.otherTrackId && track.unlocked)
                            {
                                unlock = true;
                                break;
                            }
                            else
                            {
                                unlock = false;
                            }
                        }
                        break;
                    case ConditionType.GeneralRatingClass2:
                        foreach (var track in archive.tracks)
                        {
                            
                            if (track.id == condition.otherTrackId && track.unlocked)
                            {
                                unlock = archive.isMaster; // ptt是否达标
                                foreach (var chart in track.charts)
                                {
                                    if (chart.ratingClass == 1 && chart.unlocked && chart.score >= 800000)
                                        unlock = true;
                                }
                                break;
                            }
                            else
                            {
                                unlock = false;
                            }
                        }
                        break;
                    default:
                        unlock = false;
                        break;
                }

        return unlock;
    }
    
    public static void UnlockAll(List<PackData> packs)
    {
        foreach (var pack in packs)
        {
            if (Evaluate(pack.condition)) UnlockPack(pack.id);
            foreach (var track in pack.tracks)
            {
                if (Evaluate(track.condition)) UnlockTrack(track.id);
                int index = 0;
                foreach (var chart in track.charts)
                {
                    if (Evaluate(chart.info.condition)) UnlockChart(track.id, index);
                    index++;
                }
            }
        }
        /*
        Archive archive = LoadLocalArchive();
        foreach (var trackUnlock in allUnlocks.tracks)
        {
            if (Evaluate(trackUnlock.condition)) UnlockTrack(trackUnlock.trackId);
        }
        foreach (var chartUnlock in allUnlocks.charts)
        {
            if (Evaluate(chartUnlock.condition)) UnlockChart(chartUnlock.trackId, chartUnlock.ratingClass);
        }
        foreach (var packUnlock in allUnlocks.packs)
        {
            if (Evaluate(packUnlock.condition)) UnlockPack(packUnlock.packId);
        }
        */
    }
}


