using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Profiling;

/// <summary>
/// Simple statistics for unity debug
/// デバッグ用パフォーマンスモニタ
/// </summary>
public class SimpleStats : MonoBehaviour
{
    public enum eType
    {
        Default,
        Script,
    }

    [SerializeField]
    TextMeshProUGUI ScriptHeader     = null;
    [SerializeField]
    TextMeshProUGUI FPSValue         = null;
    [SerializeField]
    TextMeshProUGUI MemoryValue      = null;
    [SerializeField]
    TextMeshProUGUI RenderValue      = null;
    [SerializeField]
    TextMeshProUGUI ScriptValue      = null;
    [SerializeField]
    GameObject      TypeDefault      = null;
    [SerializeField]
    GameObject      TypeScript       = null;
    [SerializeField]
    float           EveryDrawingTime = 0.2f;
    [SerializeField]
    eType           Type             = eType.Default;

    StringBuilder sb;
    float fps;
    float renderTime;
    float renderCallsCount;
    
    /// <summary>
    /// start
    /// </summary>
    void Start()
    {
        sb = new StringBuilder();
        
        if (ScriptHeader != null)
        {
            string text = "";
            foreach (var loop in ScriptCounter.GetPlayerLoopsInfo())
            {
                text += $"{loop.LoopName}\r\n";
            }
            ScriptHeader.SetText(text);
        }

        FPSValue?.SetText($"---");
        MemoryValue?.SetText($"---");
        RenderValue?.SetText($"---");
        ScriptValue?.SetText($"---");

        if (Debug.isDebugBuild == false)
        {
            return;
        }
        
        StartCoroutine(fpsChecker());
        StartCoroutine(renderChecker());

        StartCoroutine(draw());
    }

    /// <summary>
    /// on validate
    /// </summary>
    void OnValidate()
    {
#if UNITY_EDITOR
        if (Type == eType.Default)
        {
            TypeDefault.SetActive(true);
            TypeScript.SetActive(false);
        }
        else
        {
            TypeDefault.SetActive(false);
            TypeScript.SetActive(true);
        }
#endif
    }
    
    /// <summary>
    /// fps checker
    /// </summary>
    IEnumerator fpsChecker()
    {
        float frameCount = 0;
        float prevTime   = 0.0f;
        
        fps = 0.0f;

        while (true)
        {
            frameCount++;
            float time = Time.realtimeSinceStartup - prevTime;

            // n秒ごとに計測
            if (time >= EveryDrawingTime)
            {
                fps = frameCount / time;
            
                frameCount = 0;
                prevTime = Time.realtimeSinceStartup;
            }

            yield return null;
        
        }
    }

    /// <summary>
    /// render checker
    /// </summary>
    IEnumerator renderChecker()
    {
        Recorder recoder = Recorder.Get("Camera.Render");

        while (true)
        {
            renderTime       = recoder.elapsedNanoseconds * 0.000001f;
            renderCallsCount = recoder.sampleBlockCount;

            yield return new WaitForSeconds(EveryDrawingTime);
        }
    }

    /// <summary>
    /// draw
    /// </summary>
    IEnumerator draw()
    {
        while (true)
        {
            if (Type == eType.Default)
            {
                // fps
                FPSValue?.SetText($"{fps}");

                // memory
                if (MemoryValue != null)
                {
                    string monoheap  = (Profiler.GetMonoHeapSizeLong() / 1024).ToString("#,0");
                    string monoused  = (Profiler.GetMonoUsedSizeLong() / 1024).ToString("#,0");
                    string allocated = (Profiler.GetTotalAllocatedMemoryLong() / 1024).ToString("#,0");
                    string reserved  = (Profiler.GetTotalReservedMemoryLong() / 1024).ToString("#,0");
                    string graphics  = (Profiler.GetAllocatedMemoryForGraphicsDriver() / 1024).ToString("#,0");
                    string crlf      = System.Environment.NewLine;
                    MemoryValue?.SetText($"{monoheap} / {monoused} Kb{crlf}{allocated} / {reserved} Kb{crlf}{graphics} Kb");
                }
        
                // render
                RenderValue?.SetText($"{renderTime:F3}ms ({renderCallsCount} Calls)");
            }
            else
            if (Type == eType.Script)
            {
                // script
                if (ScriptValue != null)
                {
                    sb.Clear();
            
                    foreach (var loop in ScriptCounter.GetPlayerLoopsInfo())
                    {
                        float avg = loop.AverageTime * 1000;
                        sb.AppendLine($"{avg:F4}ms");
                    }
                    ScriptValue?.SetText(sb.ToString());
                }
            }

            yield return new WaitForSeconds(EveryDrawingTime);
        }
    }
}
