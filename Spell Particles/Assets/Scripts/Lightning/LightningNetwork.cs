using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum LightningPattern {
    Sequential,
    RandomTiered,
    FullRandom,
    RandomSpike
}

[ExecuteInEditMode]
public class LightningNetwork : MonoBehaviour {
    public KeyCode TestKey = KeyCode.Space;

    //Inspector Variables
    #region Variables
    [Header("References")]
    public LineRenderer LinePrefab;


    [Header("Behavior")]
    public float LightningDuration = 0.45f;
    [Range(0f, 1f)]
    [Tooltip("Set this to 0 for simultaneous lightning")]
    public float LeapToPointDelay = 0.08f;

    [Header("Appearance")]
    [Range(0.02f, 3f)]
    public float LineWidth = 1f;
    [Range(1, 16)]
    public int LightningSegments = 3;
    [Range(0.02f, 0.14f)]
    public float FlickerSpeed = 0.12f;
    [Range(0.25f, 3f)]
    public float LightningChaos = 1f;
    [Range(1, 8)]
    public int StrandCount = 1;

    [Header("Pattern Settings")]
    public LightningPattern PatternType = LightningPattern.Sequential;
    public float RepeatInterval = 1f; 
    [Tooltip("Set to 0 for infinite repeat")]
    public float Duration = 0f;
    public bool DoRepeat = true;
    [Header("Pattern: Spike")]
    public float SpikeLength = 2f;
    [Range(0, 30)]
    public int SpikeMidPoints = 0;
    public bool ReverseSpikes = false;


    public enum LightningShape {
        Circle,
        Sphere,
        Cone
    }
    [Header("Generate Shapes")]
    public LightningShape Shape = LightningShape.Sphere;
    public bool CreateCircle = false;
    public int pointsInCircle = 16;
    public float circleRadius = 5f;
    public bool CreateSphere = false;
    public int pointsInSphere = 128;
    public int tiersInHemisphere = 2;
    public float sphereRadius = 5f;
    public bool CreateCone = false;
    public int pointsInCone = 64;
    public int tiersInCone = 5;
    public float coneStartRadius = 1f;
    public float coneEndRadius = 5f;
    public float coneHeight = 5f;
    #endregion

    //Private
    private List<LightningPoint> Points = new List<LightningPoint>();
    private List<List<LightningPoint>> TieredPoints = new List<List<LightningPoint>>();


    private List<Coroutine> linkRoutines = new List<Coroutine>();
    private List<Coroutine> subRoutines = new List<Coroutine>();

    private bool hasGeneratedShape = false;

    //Methods
    #region MonoBehaviors
    // Start is called before the first frame update
    void Start() {
        hasGeneratedShape = false;
        if (!Application.isPlaying)
            return;

        AssignPointsInChildren();
        StartLinkPointsCoroutine();
    }

    private void StartLinkPointsCoroutine() {
        StartCoroutine(LinkPointsRepeatingCoroutine());
    }

    public void DestroyLineRenderers() {
        LineRenderer[] lines = GetComponentsInChildren<LineRenderer>();
        for (int i = 0; i < lines.Length; i++) {
            Destroy(lines[i].gameObject);
        }
    }

    private void Update() {
        UpdateAnytime();
        if (!Application.isPlaying)
            return;
        UpdateOnlyIfPlaying();
    }


    void UpdateAnytime() {
        if (CreateCircle) {
            CreateCircle = false;
            GenerateCircle(pointsInCircle, circleRadius);
        }
        if (CreateSphere) {
            CreateSphere = false;
            GenerateSphere(pointsInSphere, sphereRadius, tiersInHemisphere);
        }
        if (CreateCone) {
            CreateCone = false;
            GenerateCone(pointsInCone, coneHeight, tiersInCone, coneStartRadius, coneEndRadius);
        }
    }

    void UpdateOnlyIfPlaying() {
        
    }
    #endregion


    void ActivateCurrentLightning() {
        switch (PatternType) {
            case LightningPattern.Sequential:
                LinkPointsSequential();
                break;
            case LightningPattern.RandomTiered:
                LinkPointsRandomTiered();
                break;
            case LightningPattern.FullRandom:
                LinkPointsFullRandom();
                break;
            case LightningPattern.RandomSpike:
                LinkPointsRandomSpike();
                break;
        }

    }

    void AssignPointsInChildren() {
        Points.Clear();
        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).GetComponent<LightningPoint>()) {
                Points.Add(transform.GetChild(i).GetComponent<LightningPoint>());
            }
        }
    }

    void ListTieredPoints() {
        if (TieredPoints.Count == 0)
            Debug.Log("TieredPoints is empty");
        for (int i = 0; i < TieredPoints.Count; i++) {
            for (int j = 0; j < TieredPoints[i].Count; j++) {
                Debug.Log(i.ToString() + "," + j.ToString() + " " + TieredPoints[i][j].name);
            }
        }
    }

    #region Patterns
    //Pattern functions

    IEnumerator LinkPointsRepeatingCoroutine() {
        if (!hasGeneratedShape) {
            hasGeneratedShape = true;
            switch (Shape) {
                case LightningShape.Circle:
                    GenerateCircle();
                    break;
                case LightningShape.Sphere:
                    GenerateSphere();
                    break;
                case LightningShape.Cone:
                    GenerateCone();
                    break;

            }            
        }

        float startTime = Time.time;

        while (true) {
            while (RepeatInterval > 0f && DoRepeat && (Time.time - startTime < Duration || Duration <= 0f)) {
                ActivateCurrentLightning();
                yield return new WaitForSeconds(RepeatInterval);
            }
            yield return null;
        }
    }

    public void InterruptAllCoroutines(bool stopSubRoutines = true) {
        //for (int i = linkRoutines.Count - 1; i >= 0; i--) {
        //    StopCoroutine(linkRoutines[i]);
        //}
        //linkRoutines.Clear();
        if (stopSubRoutines) {
            for (int s = subRoutines.Count - 1; s >= 0; s--) {
                StopCoroutine(subRoutines[s]);
            }
            subRoutines.Clear();
        }
    }

    #region Sequential
    void LinkPointsSequential() {
        if (Points == null || Points.Count < 2)
            return;

        Coroutine newSubRoutine = StartCoroutine(LinkPointsSequentialCoroutine());
        subRoutines.Add(newSubRoutine);
    }

    IEnumerator LinkPointsSequentialCoroutine() {
        for (int i = 0; i < Points.Count - 1; i++) {
            LinkPoints(Points[i], Points[i + 1], LightningDuration, LightningSegments);            
            if (LeapToPointDelay > 0f) {
                yield return new WaitForSeconds(LeapToPointDelay);
            }
        }

        yield return null;
    }


    #endregion

    #region FullRandom
    void LinkPointsFullRandom() {
        if (Points == null || Points.Count < 2)
            return;

        Coroutine newSubRoutine = StartCoroutine(LinkPointsFullRandomCoroutine());
        subRoutines.Add(newSubRoutine);
    }

    IEnumerator LinkPointsFullRandomCoroutine() {
        int pointsLinked = 0;
        int currentPointIndex = Random.Range(0, Points.Count - 1);
        int nextPointIndex = Random.Range(0, Points.Count - 1);
        while (pointsLinked < Points.Count - 1) {
            LinkPoints(Points[currentPointIndex], Points[nextPointIndex], LightningDuration, LightningSegments);
            if (LeapToPointDelay > 0f) {
                yield return new WaitForSeconds(LeapToPointDelay);
            }
            currentPointIndex = nextPointIndex;
            nextPointIndex = Random.Range(0, Points.Count - 1);
            pointsLinked++;
            yield return null;
        }

        yield return null;
    }
    #endregion

    #region RandomTiered
    void LinkPointsRandomTiered() {
        if (Points == null || Points.Count < 2 || TieredPoints == null || TieredPoints.Count < 2)
            return;


        Coroutine newSubRoutine = StartCoroutine(LinkPointsRandomTieredCoroutine());
        subRoutines.Add(newSubRoutine);
    }

    IEnumerator LinkPointsRandomTieredCoroutine() {
        int pointsLinked = 0;
        //Get starting point
        int currentTier = Random.Range(0, TieredPoints.Count);
        int currentPointIndex = Points.IndexOf(GetRandomPointInTier(TieredPoints[currentTier]));
        //Get next random point
        int nextTier = currentTier;
        nextTier = (currentTier - 1 < 0 ? currentTier + 1 : currentTier + 1 > TieredPoints.Count - 1 ? currentTier - 1 : currentTier + (Random.Range(0, 2) * 2 - 1));
        int nextPointIndex = Points.IndexOf(GetRandomPointInTier(TieredPoints[nextTier]));

        while (pointsLinked < Points.Count - 1) {
            LinkPoints(Points[currentPointIndex], Points[nextPointIndex], LightningDuration, LightningSegments);
            if (LeapToPointDelay > 0f) {
                yield return new WaitForSeconds(LeapToPointDelay);
            }
            currentTier = nextTier;
            currentPointIndex = nextPointIndex;

            nextTier = (currentTier - 1 < 0 ? currentTier + 1 : currentTier + 1 > TieredPoints.Count - 1 ? currentTier - 1 : currentTier + (Random.Range(0, 2) * 2 - 1));
            nextPointIndex = Points.IndexOf(GetRandomPointInTier(TieredPoints[nextTier]));
            pointsLinked++;
            yield return null;
        }

        yield return null;
    }

    LightningPoint GetRandomPointInTier(List<LightningPoint> tier) {
        return tier[Random.Range(0, tier.Count)];
    }
    #endregion

    #region RandomSpike
    void LinkPointsRandomSpike() {
        if (Points == null || Points.Count < 2)
            return;

        Coroutine newSubRoutine = StartCoroutine(LinkPointsRandomSpikeCoroutine());
        subRoutines.Add(newSubRoutine);
    }

    IEnumerator LinkPointsRandomSpikeCoroutine() {
        LightningPoint shellPoint = Points[Random.Range(0, Points.Count)];
        Vector3 dir = (shellPoint.transform.localPosition - Vector3.zero).normalized;
        int spikePoints = SpikeMidPoints + 2;

        List<LightningPoint> tempPoints = new List<LightningPoint>();
        for (int i = 0; i < spikePoints; i++) {
            LightningPoint newPoint = CreatePoint();
            newPoint.IsTemporary = true;
            newPoint.transform.localPosition = shellPoint.transform.localPosition + dir * (SpikeLength / (float)spikePoints) * i;
            tempPoints.Add(newPoint);
        }

        LightningPoint[] tempPointArray = tempPoints.ToArray();
        if (!ReverseSpikes) {
            for (int i = 0; i < tempPointArray.Length - 1; i++) {
                LinkPoints(tempPointArray[i], tempPointArray[i + 1], LightningDuration, LightningSegments);
                if (LeapToPointDelay > 0f) {
                    yield return new WaitForSeconds(LeapToPointDelay);
                }
            }
        } else {
            for (int i = tempPointArray.Length - 1; i >= 1; i--) {
                LinkPoints(tempPointArray[i], tempPointArray[i - 1], LightningDuration, LightningSegments);
                if (LeapToPointDelay > 0f) {
                    yield return new WaitForSeconds(LeapToPointDelay);
                }
            }
        }

        yield return new WaitForSeconds(LightningDuration * (float)(tempPointArray.Length + 1));
        for (int i = 0; i < tempPointArray.Length; i++) {
            DestroyImmediate(tempPointArray[i].gameObject);
        }


        yield return null;
    }
    #endregion

    #endregion

    #region Link Points
    //Link two points functions
    void LinkPoints(LightningPoint a, LightningPoint b, float duration = 0.15f, int pieces = 3) {
        if (a != null && b != null) {
            int strands = Random.Range(1, StrandCount + 1);
            for (int i = 0; i < strands; i++) {
                Coroutine newRoutine = StartCoroutine(LinkPointsCoroutine(a, b, duration, pieces));
                linkRoutines.Add(newRoutine);
            }
        }
    }

    IEnumerator LinkPointsCoroutine(LightningPoint a, LightningPoint b, float duration, int pieces) {
        LineRenderer line = Instantiate(LinePrefab, transform);
        line.widthMultiplier = 0.15f * LineWidth;

        float startTime = Time.time;

        while (Time.time - startTime < duration) {
            GetNewLineSegments(a, b, line, pieces);
            yield return new WaitForSeconds(FlickerSpeed);
        }
        Destroy(line.gameObject);
        yield return null;
    }


    void GetNewLineSegments(LightningPoint a, LightningPoint b, LineRenderer line, int pieces) {
        if (pieces < 1 || a == null || b == null || line == null)
            return;

        line.positionCount = pieces + 1;
        Vector3[] newPositions = new Vector3[line.positionCount];
        int first = 0;
        int last = newPositions.Length - 1;

        Vector3 aPos = newPositions[first] = a.transform.localPosition;
        Vector3 bPos = newPositions[last] = b.transform.localPosition;

        float segmentLength = (Vector3.Distance(bPos, aPos) / (float)(newPositions.Length - 1));


        for (int i = 1; i < newPositions.Length - 1; i++) {
            Vector3 alignedPoint = aPos + (bPos - aPos).normalized * segmentLength * i;
            newPositions[i] = GetRandomPointFromCenter(alignedPoint, segmentLength, LightningChaos);
        }

        line.SetPositions(newPositions);
    }

    Vector3 GetRandomPointFromCenter(Vector3 center, float segmentLength, float extents = 0.8f) {
        return center + Random.insideUnitSphere * (segmentLength / 2f) * extents;
    }
    #endregion

    #region Generate Shapes
    //Generate pre-defined shapes

    #region Circle
    void GenerateCircle(int points = 16, float radius = 5f) {
        if (points <= 2)
            return;
        //Create children
        //Assign children to Points list
        //position Children
        DestroyExistingLightingPoints();
        CreatePoints(points);
        AssignPointsInChildren();
        //Math
        ArrangeCirclePoints(Points.ToArray(), radius);
    }

    void ArrangeCirclePoints(LightningPoint[] pointsToArrange, float radius, float height = 0f) {
        if (pointsToArrange == null || pointsToArrange.Length == 0)
            return;

        string debugTxt = "Arranging points : ";
        for (int i = 0; i < pointsToArrange.Length && pointsToArrange[i] != null; i++) {
            debugTxt += pointsToArrange[i].name + ", ";
        }
        //Debug.Log(debugTxt + " at height " + height.ToString());
        for (int i = 0; i < pointsToArrange.Length; i++) {
            if (pointsToArrange[i] != null) {
                float normalize = (float)i / (float)pointsToArrange.Length;
                normalize = 1f - normalize;
                float nextX = normalize * 2 * Mathf.PI;
                Vector3 nextPosition = new Vector3(Mathf.Cos(nextX), 0f, Mathf.Sin(nextX));
                nextPosition = nextPosition.normalized * radius;                
                pointsToArrange[i].transform.localPosition = new Vector3(nextPosition.x, height, nextPosition.z);
            }
        }
    }

    void ArrangeCirclePointsInSpiral(LightningPoint[] pointsToArrange, float startRadius, float endRadius, float startHeight, float endHeight) {
        if (pointsToArrange == null || pointsToArrange.Length == 0)
            return;

        string debugTxt = "Arranging points : ";
        for (int i = 0; i < pointsToArrange.Length && pointsToArrange[i] != null; i++) {
            debugTxt += pointsToArrange[i].name + ", ";
        }
        //Debug.Log(debugTxt + " at height " + height.ToString());
        for (int i = 0; i < pointsToArrange.Length; i++) {
            if (pointsToArrange[i] != null) {
                float normalize = (float)i / (float)pointsToArrange.Length;
                float clockwiseNormalize = 1f - normalize;
                float nextX = clockwiseNormalize * 2 * Mathf.PI;
                Vector3 nextPosition = new Vector3(Mathf.Cos(nextX), 0f, Mathf.Sin(nextX));
                nextPosition = nextPosition.normalized * Mathf.Lerp(startRadius, endRadius, normalize);
                float finalHeight = Mathf.Lerp(startHeight, endHeight, normalize);
                pointsToArrange[i].transform.localPosition = new Vector3(nextPosition.x, finalHeight, nextPosition.z);
            }
        }
    }
    #endregion

    #region Sphere
    void GenerateSphere() {
        GenerateSphere(pointsInSphere, sphereRadius, tiersInHemisphere);
    }

    void GenerateSphere(int points, float radius, int tiers) {
        if (points <= 5)
            return;

        DestroyExistingLightingPoints();
        CreatePoints(points);
        AssignPointsInChildren();

        //0 tiers = top points + equator
        //1 tiers = 1 mid circle
        TieredPoints.Clear();
        ArrangeSpherePoints(Points.ToArray(), radius, 4 + tiers * 2 - 1);
    }

    void ArrangeSpherePoints(LightningPoint[] pointsToArrange, float radius, int tiers) {
        int pointIndex = 0;
        float heightPerTier = (radius * 2f / (float)(tiers - 1f));
        //int totalTiers = tiers * 2 - 1;        
        for (int i = 0; i < tiers; i++) {
            //for each tier
            float distanceFromMiddle = Mathf.Floor(tiers / 2f) - i;
            float tierHeight = heightPerTier * distanceFromMiddle;
            if ((i == 0 || i == tiers - 1)) {
                //Debug.Log("Arranging points : " + pointsToArrange[pointIndex].name + " at height " + tierHeight.ToString());
                pointsToArrange[pointIndex].transform.localPosition = new Vector3(0f, tierHeight, 0f);
                AddPointsToTier(new LightningPoint[1] { pointsToArrange[pointIndex] }, TieredPoints);
                pointIndex += 1;
            } else {
                float tierRadius = Mathf.Sqrt(Mathf.Pow(radius, 2) - Mathf.Pow(Mathf.Abs(tierHeight), 2));

                int tierPointCount = PointsPerTier(pointsToArrange.Length, tiers);
                int remainder = (pointsToArrange.Length - 2) % tierPointCount;
                //The middle tier gets any remainder points
                LightningPoint[] tierPoints = GetGroupOfPoints(pointsToArrange, pointIndex, (distanceFromMiddle != 0f ? pointIndex + tierPointCount - 1 : pointIndex + tierPointCount - 1 + remainder));
                AddPointsToTier(tierPoints, TieredPoints);
                ArrangeCirclePoints(tierPoints, tierRadius, tierHeight);
                pointIndex += (distanceFromMiddle != 0f ? tierPointCount : tierPointCount + remainder);
            }
        }
    }
    #endregion

    #region Cone
    void GenerateCone() {
        GenerateCone(pointsInCone, coneHeight, tiersInCone, coneStartRadius, coneEndRadius);
    }

    void GenerateCone(int points, float height, int tiers, float startRadius, float endRadius) {
        if (points <= 5)
            return;

        DestroyExistingLightingPoints();
        CreatePoints(points);
        AssignPointsInChildren();

        //0 tiers = top points + equator
        //1 tiers = 1 mid circle
        TieredPoints.Clear();
        ArrangeConePoints(Points.ToArray(), height, tiers, startRadius, endRadius);
    }


    void ArrangeConePoints(LightningPoint[] pointsToArrange, float height, int tiers, float startRadius, float endRadius) {
        int pointIndex = 0;
        float heightPerTier = (height / (float)(tiers - 1));
        //int totalTiers = tiers * 2 - 1;        
        for (int i = 0; i < tiers; i++) {
            //for each tier
            float tierHeight = heightPerTier * i;
            float nextTierHeight = heightPerTier * (i + 1);
            float tierRadius = Mathf.Lerp(startRadius, endRadius, (i / (float)(tiers - 1)));
            float nextTierRadius = Mathf.Lerp(startRadius, endRadius, ((i + 1) / (float)(tiers - 1)));

            int tierPointCount = (int)Mathf.Floor(pointsToArrange.Length / (float)tiers);
            int remainder = (pointsToArrange.Length) % tierPointCount;
            //The middle tier gets any remainder points
            LightningPoint[] tierPoints = GetGroupOfPoints(pointsToArrange, pointIndex, i == tiers - 1 ? pointIndex + tierPointCount - 1 + remainder : pointIndex + tierPointCount - 1);
            AddPointsToTier(tierPoints, TieredPoints);
            ArrangeCirclePointsInSpiral(tierPoints, tierRadius, nextTierRadius, tierHeight, nextTierHeight);
            pointIndex += tierPointCount;            
        }
    }
    #endregion

    void AddPointsToTier(LightningPoint[] points, List<List<LightningPoint>> listOfTierLists) {
        if (points.Length == 0 || points == null || listOfTierLists == null) {
            Debug.Log("No tier data to add to.");
            return;
        }

        listOfTierLists.Add(new List<LightningPoint>());
        for (int i = 0; i < points.Length; i++) {
            listOfTierLists[listOfTierLists.Count - 1].Add(points[i]);
        }
    }

    LightningPoint[] GetGroupOfPoints(LightningPoint[] sourceArray, int firstPoint, int lastPoint) {
        LightningPoint[] array = new LightningPoint[lastPoint - firstPoint + 1];
        for (int i = 0; i < array.Length && sourceArray[firstPoint + i] != null; i++) {
            array[i] = sourceArray[firstPoint + i];
        }

        return array;
    }



    int PointsPerTier(int totalPoints, int tiers) {
        return (totalPoints - 2) / (tiers - 2);
    }

    void DestroyExistingLightingPoints() {
        AssignPointsInChildren();
        if (Points != null && Points.Count > 0) {
            LightningPoint[] allPoints = Points.ToArray();
            for (int i = 0; i < allPoints.Length; i++) {
                DestroyImmediate(allPoints[i].gameObject);
            }
            Points.Clear();
        }
    }

    void CreatePoints(int numberOfPoints) {
        for (int i = 0; i < numberOfPoints; i++) {
            CreatePoint();
        }
    }


    LightningPoint CreatePoint() {
        LightningPoint newPoint = new GameObject("newLightningPoint").AddComponent<LightningPoint>();
        newPoint.transform.SetParent(transform);
        newPoint.transform.localPosition = Vector3.zero;
        newPoint.name = newPoint.name + " " + newPoint.transform.GetSiblingIndex().ToString();

        return newPoint;
    }
    #endregion

    #region Gizmos
    //Gizmos
    private void OnDrawGizmos() {
        if (Application.isPlaying || Random.Range(0.0f,1.0f) > 0.45f)
            return;

        Color lineColor = new Color(0.85f, 0.0f, 1f, 1f);
        Gizmos.color = lineColor;


        for (int i = 0; i < transform.childCount - 1; i++) {
            if (transform.GetChild(i).GetComponent<LightningPoint>()) {
                if (!transform.GetChild(i).GetComponent<LightningPoint>().IsTemporary) {
                    Gizmos.DrawLine(transform.GetChild(i).position, NextLightningChild(i, i + 1));
                    if (i != 0) Gizmos.DrawSphere(transform.GetChild(i).position, 0.05f);
                }
            }
        }
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(FirstLightningChild(), 0.15f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(LastLightningChild(), 0.15f);

        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).GetComponent<LightningPoint>()) {
                if (transform.GetChild(i).GetComponent<LightningPoint>().IsTemporary) {
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireSphere(transform.GetChild(i).position, 0.04f);
                }
            }
        }
    }

    Vector3 NextLightningChild(int self = 0, int nextToTest = 1) {
        if (transform.childCount == 0 || transform.GetChild(nextToTest) == null)
            return Vector3.zero;
        if (transform.childCount == 1)
            return transform.GetChild(0).position;



        Vector3 posIfNoLightningFound = (nextToTest == transform.childCount - 1 ? (transform.GetChild(self) == null ? Vector3.zero : transform.GetChild(self).position) : NextLightningChild(self, nextToTest + 1));


        return (transform.GetChild(nextToTest).GetComponent<LightningPoint>() != null && !transform.GetChild(nextToTest).GetComponent<LightningPoint>().IsTemporary ? transform.GetChild(nextToTest).position : posIfNoLightningFound);
    }

    Vector3 LastLightningChild() {
        if (transform.childCount == 0)
            return Vector3.zero;

        for (int i = transform.childCount - 1; i >= 0; i--) {
            if (transform.GetChild(i).GetComponent<LightningPoint>() != null && !transform.GetChild(i).GetComponent<LightningPoint>().IsTemporary)
                return transform.GetChild(i).position;
        }

        return Vector3.zero;
    }


    Vector3 FirstLightningChild() {
        if (transform.childCount == 0)
            return Vector3.zero;

        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).GetComponent<LightningPoint>() != null && !transform.GetChild(i).GetComponent<LightningPoint>().IsTemporary)
                return transform.GetChild(i).position;
        }

        return Vector3.zero;
    }
    #endregion

}
