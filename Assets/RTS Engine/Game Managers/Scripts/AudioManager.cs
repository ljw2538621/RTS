using UnityEngine;
using System.Collections;

/* Audio Manager script created by Oussama Bouanani, SoumiDelRio.
 * This script is part of the Unity RTS Engine */

namespace RTSEngine
{
	public class AudioManager : MonoBehaviour {

		public static void Play (AudioSource source, AudioClip clip, bool loop = false)
		{
            if (source == null || clip == null) //in case no source or no clip has been assigned 
                return;

            source.Stop (); //stop the current audio clip from playing.

            source.clip = clip;
            source.loop = loop;

            source.Play (); //play the new clip
		}

		public static void Stop (AudioSource source)
		{
            if (source == null)
                return;

            source.Stop();
		}
	}
}