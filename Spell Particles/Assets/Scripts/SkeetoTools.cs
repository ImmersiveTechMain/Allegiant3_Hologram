using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Diagnostics;
using System.IO;
using Debug = UnityEngine.Debug;
public static class JoystickInput
{

    public static bool JoystickTriggerInUse(float deadZone = 0.1f)
    {
        return Mathf.Abs(Input.GetAxis("Main Joystick Trigger")) > deadZone;
    }

}

public static class SkeetoTools
{
    public class Timer
    {
        // Statics 
        public static Transform TimersFolder { private set; get; }

        // Object
        public class Obj : MonoBehaviour { }

        // Public Variables
        public float duration { private set; get; }
        public float timeRemaining { private set; get; }
        public float timePassed { get { return duration - timeRemaining; } }
        public bool isRunning { private set; get; }
        public bool isPaused { private set; get; }

        // Callbacks
        public delegate void CALLBACK();
        public CALLBACK OnTimerEnds = delegate () { };
        public CALLBACK OnTimerPaused = delegate () { };
        public CALLBACK OnTimerResumes = delegate () { };
        public CALLBACK OnTick = delegate () { };

        //private 
        private Obj obj;

        // Run function will only work if timer is not running already. it will unpause and start the timer from duration.
        public void Run()
        {
            if (!isRunning)
            {
                isRunning = true;
                isPaused = false;
                obj.StopAllCoroutines();
                obj.StartCoroutine(TimerCoroutine());
            }
        }

        // Stop will reset the timer to duration and you will have to call Run to start it again. works as a reset.
        public void Stop()
        {
            isRunning = false;
            obj.StopAllCoroutines();
        }

        // pauses the timer use resume to unpause
        public void Pause()
        {
            bool wasUnpaused = isRunning && !isPaused;
            isPaused = true;
            if (wasUnpaused) { OnTimerPaused(); }
        }

        // unpauses the timer.
        public void Resume()
        {
            bool wasPaused = isRunning && isPaused;
            isPaused = false;
            if (wasPaused) { OnTimerResumes(); }
        }

        public Timer(float duration)
        {
            this.duration = duration;
            this.timeRemaining = duration;
            CreateTimerObj();
        }

        IEnumerator TimerCoroutine()
        {

            timeRemaining = duration;
            while (timeRemaining > 0)
            {

                if (isRunning && !isPaused)
                {

                    timeRemaining -= Time.deltaTime;
                    timeRemaining = Mathf.Clamp(timeRemaining, 0f, float.MaxValue);
                    OnTick();
                    yield return null;
                }
                else { yield return null; }
            }
            OnTimerEnds();
        }

        void CreateStaticFolder()
        {
            if (TimersFolder == null)
            {
                TimersFolder = new GameObject("Timer Objects").transform;
                UnityEngine.Object.DontDestroyOnLoad(TimersFolder);
                TimersFolder.position = Vector3.zero;
            }
        }

        void CreateTimerObj()
        {
            CreateStaticFolder();
            Obj obj = new GameObject("Timer [" + (TimersFolder.childCount + 1) + "]").AddComponent<Obj>();
            obj.transform.SetParent(TimersFolder, false);
            obj.transform.localPosition = Vector3.zero;
            this.obj = obj;
        }

        public static string FormatToDisplay(float time, bool showHours = false, bool showMilliseconds = false, bool showMinutes = false)
        {
            int microSeconds = (int)((time - Mathf.Floor(time)) * 100);
            int seconds = (int)Mathf.Repeat(Mathf.FloorToInt(time), 60);
            int minutes = (int)Mathf.Repeat((Mathf.FloorToInt(time) / 60f), 60);
            int hours = (int)(((Mathf.FloorToInt(time) / 60f) / 60f));

            string ms = (microSeconds < 10 ? "0" : "") + microSeconds.ToString();
            string sec = (seconds < 10 ? "0" : "") + seconds.ToString();
            string min = (minutes < 10 ? "0" : "") + minutes.ToString();
            string h = (hours < 10 ? "0" : "") + hours.ToString();

            return (showHours ? h + ":" : "") + (showMinutes ? min + ":" : "") + sec + (showMilliseconds ? (":" + ms) : "");

        }

        ~Timer()
        {
            if (obj != null && obj.gameObject != null) { UnityEngine.Object.Destroy(obj.gameObject); }
        }
    }

    public static bool IsAllTrue(this bool[] boolArray) { if (boolArray == null || boolArray.Length <= 0) { return false; } bool allTrue = true; for (int i = 0; i < boolArray.Length; allTrue &= boolArray[i++]) ; return allTrue; }

    public static void DestroyGameObjectArray<T>(this T[] array) where T : Component
    {
        if (array != null)
        {
            for (int i = 0; i < array.Length; i++) { if (array[i] != null) { GameObject.Destroy(array[i].gameObject); } }
        }
        array = null;
    }

    public static void DestroyGameObject2DArray<T>(this T[,] array) where T : Component
    {
        if (array != null)
        {
            for (int x = 0; x < array.GetLength(0); x++)
            {
                for (int y = 0; y < array.GetLength(1); y++)
                {
                    if (array[x, y] != null) { GameObject.Destroy(array[x, y].gameObject); }
                }
            }
        }
        array = null;
    }

    public static Gradient MixWith(this Gradient A, Gradient B, float strength = 0.5f)
    {
        int colorKeyCount = Mathf.Max(A.colorKeys.Length, B.colorKeys.Length);
        int alphaKeyCount = Mathf.Max(A.alphaKeys.Length, B.alphaKeys.Length);
        GradientColorKey[] resulColortKeys = new GradientColorKey[colorKeyCount];
        GradientAlphaKey[] resultAlphaKeys = new GradientAlphaKey[alphaKeyCount];

        for (int i = 0; i < colorKeyCount; i++)
        {
            bool aContainsI = i < A.colorKeys.Length;
            bool bContainsI = i < B.colorKeys.Length;
            bool hasMatch = aContainsI && bContainsI;

            Color mixedColor = hasMatch ? Color.Lerp(A.colorKeys[i].color, B.colorKeys[i].color, strength) : bContainsI ? B.colorKeys[i].color : A.colorKeys[i].color;
            float mixedColorTime = hasMatch ? Mathf.Lerp(A.colorKeys[i].time, B.colorKeys[i].time, strength) : bContainsI ? B.colorKeys[i].time : A.colorKeys[i].time;
            GradientColorKey nextColor = new GradientColorKey(mixedColor, mixedColorTime);
            resulColortKeys[i] = nextColor;
        }

        for (int i = 0; i < alphaKeyCount; i++)
        {
            bool aContainsI = i < A.alphaKeys.Length;
            bool bContainsI = i < B.alphaKeys.Length;
            bool hasMatch = aContainsI && bContainsI;

            float mixedAlpha = hasMatch ? Mathf.Lerp(A.alphaKeys[i].alpha, B.alphaKeys[i].alpha, strength) : bContainsI ? B.alphaKeys[i].alpha : A.alphaKeys[i].alpha;
            float mixedAlphaTime = hasMatch ? Mathf.Lerp(A.alphaKeys[i].time, B.alphaKeys[i].time, strength) : bContainsI ? B.alphaKeys[i].time : A.alphaKeys[i].time;
            GradientAlphaKey nextAlpha = new GradientAlphaKey(mixedAlpha, mixedAlphaTime);

            resultAlphaKeys[i] = nextAlpha;
        }

        Gradient result = new Gradient();
        result.alphaKeys = resultAlphaKeys;
        result.colorKeys = resulColortKeys;
        return result;
    }

    public static void SetSceneInputNavigationState(UnityEngine.UI.Navigation.Mode mode)
    {

        UnityEngine.UI.Selectable[] inputs = UnityEngine.Object.FindObjectsOfType<UnityEngine.UI.Selectable>();
        if (inputs != null && inputs.Length > 0)
        {
            for (int i = 0; i < inputs.Length; i++)
            {
                Navigation currentNavigation = inputs[i].navigation;
                currentNavigation.mode = mode;
                inputs[i].navigation = currentNavigation;
            }
        }
    }

    public static Coroutine LoopCoroutine(this MonoBehaviour mono, float intervalDuration, Action<float> action, Action onLoopPointReachedAction = null, Func<bool> whileCondition = null)
    {
        if (mono != null)
        {
            return mono.StartCoroutine(_LoopCoroutine(intervalDuration, action, onLoopPointReachedAction, whileCondition));
        }
        else
        {
            return null;
        }
    }

    static IEnumerator _LoopCoroutine(float intervalDuration, Action<float> action, Action onLoopPointReachedAction = null, Func<bool> whileCondition = null)
    {
        float counter = 0;
        bool hasWhileCondition = whileCondition != null;
        while (!hasWhileCondition || whileCondition())
        {
            counter = 0;
            while (counter < intervalDuration && (!hasWhileCondition || whileCondition()))
            {
                float normalized = counter / intervalDuration;
                if (action != null) { action(normalized); }
                counter += Time.deltaTime;
                yield return null;
            }
            if (action != null) { action(1); }
            if (onLoopPointReachedAction != null) { onLoopPointReachedAction(); }
            yield return null;
        }
    }

    public static Coroutine InterpolateCoroutine(this MonoBehaviour mono, float duration, Action<float>[] actions, Action[] then = null)
    {
        bool done = false;
        WaitUntil response = new WaitUntil(() => { return done; });
        Action[] _then = new Action[then == null ? 1 : then.Length + 1];
        for (int i = 0; then != null && i < then.Length; i++)
        {
            _then[i] = then[i];
        }
        _then[_then.Length - 1] = () => { done = true; };
        return mono.StartCoroutine(_InterpolateCoroutine(duration, actions, _then));
    }

    public static Coroutine InterpolateCoroutine(this MonoBehaviour mono, float duration, Action<float> action, Action then = null)
    {
        return (InterpolateCoroutine(mono, duration, action != null ? new Action<float>[1] { action } : null, then != null ? new Action[1] { then } : null));
    }

    static IEnumerator _InterpolateCoroutine(float duration, Action<float>[] actions, Action[] then = null)
    {
        float counter = 0;
        while (counter < duration)
        {
            float normalized = counter / duration;

            for (int i = 0; actions != null && i < actions.Length; i++)
            {
                if (actions[i] != null) { actions[i](normalized); }
            }

            counter += Time.deltaTime;
            yield return null;
        }
        for (int i = 0; actions != null && i < actions.Length; i++)
        {
            if (actions[i] != null) { actions[i](1); }
        }
        yield return null;
        for (int i = 0; then != null && i < then.Length; i++)
        {
            if (then[i] != null) { then[i](); }
        }
    }

    public static UnityEngine.UI.Graphic Transparency(this UnityEngine.UI.Graphic graphic, float alpha)
    {
        Color c = graphic.color;
        c.a = alpha;
        graphic.color = c;
        return graphic;
    }

    public static Coroutine ActionAfterCondition(this MonoBehaviour mono, Func<bool> condition, Action then)
    {
        return mono.StartCoroutine(_ActionAfterCondition(condition, then));
    }

    public static void ActionAfterFrameDelay(this MonoBehaviour mono, int frames, Action action)
    {
        mono.StartCoroutine(_ActionAfterFrameDelay(frames, action));
    }

    public static Coroutine ActionAfterSecondDelay(this MonoBehaviour mono, float seconds, Action action)
    {
        return mono.StartCoroutine(_ActionAfterSecondDelay(seconds, action));
    }

    public static string[] SortAlphabet(this string[] array)
    {
        List<string> options = new List<string>();
        options.AddRange(array);
        options.Sort();
        return options.ToArray();
    }

    static IEnumerator _ActionAfterFrameDelay(int frames, Action action)
    {
        for (int i = 0; i < frames; i++) { yield return null; }
        if (action != null) { action(); }
    }

    static IEnumerator _ActionAfterSecondDelay(float seconds, Action action)
    {
        yield return new WaitForSecondsRealtime(seconds);
        if (action != null) { action(); }
    }

    static IEnumerator _ActionAfterCondition(Func<bool> condition, Action then)
    {
        yield return new WaitUntil(condition);
        if (then != null) { then(); }
    }


    public static bool WriteFile(string fullPath, string content) // must include extention of file
    {
        string directoryPath = Path.GetDirectoryName(fullPath);

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        try
        {
            File.WriteAllText(fullPath, content);
            return true;
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            return false;
        }
    }

    public static string ReadFile(string fullPath) // must include extention of file
    {
        string directoryPath = Path.GetDirectoryName(fullPath);
        if (!Directory.Exists(directoryPath) || !File.Exists(fullPath)) { return "Error, file at (" + fullPath + ") couldn't be found."; }
        string result = "";
        try
        {
            result = File.ReadAllText(fullPath);
            return result;
        }
        catch (System.Exception e)
        {
            Debug.Log("Error" + e.Message);
            return e.Message;
        }
    }

    public static bool CreateFileIfUnexistent(string fullPath_withFileName_and_extension, string content)
    {
        CreateDirectoryForPath(fullPath_withFileName_and_extension);
        if (!File.Exists(fullPath_withFileName_and_extension))
        {
           return WriteFile(fullPath_withFileName_and_extension, content);
        }
        return false;
    }

    public static int GetFilesCountInDirectory(string directoryPath, string extention = null)
    {
        if (!Directory.Exists(directoryPath)) { Debug.Log("Error, directory (" + directoryPath + ") couldn't be found."); return 0; }

        string[] files = GetFilesNameInDirectory(directoryPath, extention);
        return files.Length;
    }

    public static string[] GetFilesNameInDirectory(string directoryPath, string extention = null, SearchOption searchOption = SearchOption.TopDirectoryOnly, bool includeExtension = true)
    {
        if (!Directory.Exists(directoryPath))
        {
            string error = "Error, directory (" + directoryPath + ") couldn't be found.";
            Debug.Log(error);
            return new string[1] { error };
        }

        string[] result;
        try
        {
            result = Directory.GetFiles(directoryPath, extention??"*", SearchOption.AllDirectories);
            if (!includeExtension && result != null) {
                for (int i = 0; i < result.Length; i++) {
                    string[] pieces = result[i].Split('.');
                    result[i] = pieces[0];
                }
            }
            return result;
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            return new string[1] { e.Message };
        }
    }

    public enum NormalizeWaveType
    {
        Lineal,
        SlowToFast,
        FastToSlow,
        SlowFastSlow,
        SlowToLineal,
        LinealToSlow,
        LinealToFast,
        FastToLineal
    }
    public static float GetNormalizedValueFromWave(this float linealNormalization, NormalizeWaveType waveType)
    {

        float fastToSlowWave = Mathf.Sqrt(linealNormalization);
        float slowToFastWave = linealNormalization * linealNormalization;
        float linealWave = linealNormalization;

        Vector2 fastToSlowSample = new Vector2(linealNormalization, Mathf.Sqrt(linealNormalization)) - new Vector2(Mathf.Clamp(linealNormalization - Time.deltaTime, 0, float.MaxValue), Mathf.Sqrt(Mathf.Clamp(linealNormalization - Time.deltaTime, 0, float.MaxValue)));
        Vector2 slowToFastSample = new Vector2(linealNormalization, linealNormalization * linealNormalization) - new Vector2(Mathf.Clamp(linealNormalization - Time.deltaTime, 0, float.MaxValue), Mathf.Pow(Mathf.Clamp(linealNormalization - Time.deltaTime, 0, float.MaxValue), 2));
        Vector2 linealSample = Vector3.one * Time.deltaTime;

        fastToSlowSample.Normalize();
        slowToFastSample.Normalize();
        linealSample.Normalize();

        float result = 0;

        switch (waveType)
        {
            case NormalizeWaveType.FastToSlow: result = Mathf.Sqrt(linealNormalization); break;
            case NormalizeWaveType.Lineal: result = linealNormalization; break;
            case NormalizeWaveType.LinealToFast: result = slowToFastSample.y < linealSample.y ? linealWave : slowToFastWave; break;
            case NormalizeWaveType.LinealToSlow: result = fastToSlowSample.y > linealSample.y ? linealWave : fastToSlowWave; break;
            case NormalizeWaveType.SlowFastSlow: result = Mathf.SmoothStep(0.0f, 1.0f, linealNormalization); break;
            case NormalizeWaveType.SlowToFast: result = linealNormalization * linealNormalization; break;
            case NormalizeWaveType.SlowToLineal: result = slowToFastSample.y < linealSample.y ? slowToFastWave : linealWave; break;
            case NormalizeWaveType.FastToLineal: result = fastToSlowSample.y > linealSample.y ? fastToSlowWave : linealWave; break;
            default: result = linealWave; break;
        }
        return Mathf.Clamp(result, 0, 1);
    }


    public static void CreateDirectoryForPath(string path)
    {
        string directoryPath = Path.GetDirectoryName(path);
        if (!Directory.Exists(directoryPath)) { Directory.CreateDirectory(directoryPath); }
    }

    public static string GetDate()
    {
        System.DateTime time = System.DateTime.Now;
        return time.ToString("D", System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
    }

    public static void TakeScreenshot(Camera cam, string path, Action then)
    {
        ScreenshotTaker st = cam.GetComponent<ScreenshotTaker>();
        if (st == null) { st = cam.gameObject.AddComponent<ScreenshotTaker>(); }
        st.Set(cam);
        st.TakeScrenshot(path, then);
    }


    public static Coroutine UITypeLetterByLetter(this MonoBehaviour mono, TMPro.TextMeshProUGUI textOBJ, string text, bool append = false, int framePerLetter = 1, Action then = null, bool textMeshPro_TagsAsSingle = true)
    {
        if (mono == null || textOBJ == null) { return null; }
        return mono.StartCoroutine(_UITypeLetterByLetter(textOBJ, text, append, framePerLetter, then, textMeshPro_TagsAsSingle));
    }

    public static Coroutine UITypeLetterByLetter(this MonoBehaviour mono, UnityEngine.UI.Text textOBJ, string text, bool append = false, int framePerLetter = 1, Action then = null)
    {
        if (mono == null || textOBJ == null) { return null; }
        return mono.StartCoroutine(_UITypeLetterByLetter(textOBJ, text, append, framePerLetter, then));
    }

    public static Coroutine UIRemoveLetterByLetter(this MonoBehaviour mono, UnityEngine.UI.Text textOBJ, int framePerLetter = 1, Action then = null)
    {
        return mono.StartCoroutine(_UIRemoveLetterByLetter(textOBJ, framePerLetter, then));
    }

    public static Coroutine UIRemoveLetterByLetter(this MonoBehaviour mono, TMPro.TextMeshProUGUI textOBJ, int framePerLetter = 1, Action then = null)
    {
        return mono.StartCoroutine(_UIRemoveLetterByLetter(textOBJ, framePerLetter, then));
    }

    static IEnumerator _UIRemoveLetterByLetter(TMPro.TextMeshProUGUI textOBJ, int framePerLetter = 1, Action then = null)
    {
        while (textOBJ.text.Length > 0)
        {
            textOBJ.text = textOBJ.text.Substring(0, textOBJ.text.Length - 1);
            for (int i = 0; i < framePerLetter; i++) { yield return null; }
        }
        if (then != null) { then(); }
    }

    static IEnumerator _UIRemoveLetterByLetter(UnityEngine.UI.Text textOBJ, int framePerLetter = 1, Action then = null)
    {
        while (textOBJ.text.Length > 0)
        {
            textOBJ.text = textOBJ.text.Substring(0, textOBJ.text.Length - 1);
            for (int i = 0; i < framePerLetter; i++) { yield return null; }
        }
        if (then != null) { then(); }
    }

    static IEnumerator _UITypeLetterByLetter(UnityEngine.UI.Text textOBJ, string text, bool append = false, int framePerLetter = 1, Action then = null)
    {
        string finalText = append ? (textOBJ.text + text) : text;
        if (!append) { textOBJ.text = ""; }

        int initialIndex = textOBJ.text.Length;
        int counter = 0;

        while (textOBJ.text.Length < finalText.Length)
        {
            string next = finalText.Substring(0, initialIndex + ++counter);
            textOBJ.text = next;
            for (int i = 0; i < framePerLetter; i++) { yield return null; }
        }
        if (then != null) { then(); }
    }

    static IEnumerator _UITypeLetterByLetter(TMPro.TextMeshProUGUI textOBJ, string text, bool append = false, int framePerLetter = 1, Action then = null, bool textMeshPro_TagsAsSingle = true)
    {
        char boldOpen_EncryptedChar = '`';
        char boldClose_EncryptedChar = '~';

        text = textMeshPro_TagsAsSingle ? (text.Replace("<b>", boldOpen_EncryptedChar.ToString()).Replace("</b>", boldClose_EncryptedChar.ToString())) : text;

        string textObjectFirstParameterText = textMeshPro_TagsAsSingle ? textOBJ.text.Replace("<b>", boldOpen_EncryptedChar.ToString()).Replace("</b>", boldClose_EncryptedChar.ToString()) : textOBJ.text;

        string finalText = append ? textObjectFirstParameterText + text : text;
        string nextAcumulatedText = "";
        if (!append) { textOBJ.text = ""; textObjectFirstParameterText = ""; }

        int initialIndex = textObjectFirstParameterText.Length;
        int counter = 0;

        while (nextAcumulatedText.Length < finalText.Length)
        {
            string next = finalText.Substring(0, initialIndex + ++counter);
            nextAcumulatedText = next;
            textOBJ.text = textMeshPro_TagsAsSingle ? next.Replace(boldOpen_EncryptedChar.ToString(), "<b>").Replace(boldClose_EncryptedChar.ToString(), "</b>") : next;
            for (int i = 0; i < framePerLetter; i++) { yield return null; }
        }
        if (then != null) { then(); }
    }

    public class ScreenshotTaker : MonoBehaviour
    {
        Camera cam;

        public void Set(Camera cam)
        {
            this.cam = cam;
        }

        public void TakeScrenshot(string path, Action then)
        {
            StartCoroutine(_TakeScreenshot(path, then));
        }

        IEnumerator _TakeScreenshot(string path, Action then)
        {
            yield return new WaitForEndOfFrame();
            if (this.cam != null && !string.IsNullOrEmpty(path))
            {
                CreateDirectoryForPath(path);
                try
                {
                    RenderTexture rt = new RenderTexture(cam.pixelWidth, cam.pixelHeight, 24);
                    cam.targetTexture = rt;
                    cam.Render();
                    Graphics.SetRenderTarget(rt);
                    Texture2D texture = new Texture2D(cam.pixelWidth, cam.pixelHeight, TextureFormat.RGB24, false);
                    texture.ReadPixels(cam.pixelRect, 0, 0);
                    texture.Apply();


                    byte[] parsed = texture.EncodeToPNG();
                    File.WriteAllBytes(path, parsed);
                }
                catch (System.Exception e)
                {
                    Debug.Log("Error at taking screenshot: " + e.Message);
                }
                if (then != null) { then(); }
            }
            else
            {
                Debug.Log("You first need to set the camera and/or provide a valid path");
            }
        }



    }
}






