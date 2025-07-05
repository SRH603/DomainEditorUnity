namespace Battlehub.Storage.Enumerators.UnityEngine
{
    [ObjectEnumerator(typeof(global::UnityEngine.AudioSource))]
    public class AudioSourceEnumerator : ObjectEnumerator<global::UnityEngine.AudioSource>
    {
        public override bool MoveNext()
        {
            do
            {
                switch (Index)
                {

                    case 0:
                        if (MoveNext(TypedObject.clip, 7))
                            return true;
                        break;
                    case 1:
                        if (MoveNext(TypedObject.outputAudioMixerGroup, 8))
                            return true;
                        break;
                    case 2:
                        if (MoveNext(Object, -1))
                            return true;
                        break;
                    default:
                        return false;
                }
            }
            while (true);
        }
    }
}