using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.IO;

public class MainGenerator : MonoBehaviour {

	//public WorkingBlink blink;
	public GameObject scanOrigin;
	public float samplingFreq;
	public int horLaser, verLaser;
	[Range(1, 90)] public int verViewField;
    public string outputFilename = "output.pts";

	private float timer, period;
	private List<Vector3> dirs;
	private List<Vector3> pts;
	private bool working;
	private FileStream fs;
	private StreamWriter w; 

	// Use this for initialization
	void Start () 
	{
		working = false;
		period = 100;
		timer = 0;
		dirs = new List<Vector3>();
		pts = new List<Vector3>();
	}
	
	// Update is called once per frame
	void Update () {
		if (working)
		{
			timer += Time.deltaTime;
			if (timer > period)
			{
				timer -= period;
				GetPoints(scanOrigin.transform.position);
			}
		}

        if (Input.GetButtonDown("Cancel"))
        {
            Debug.Log("Scanning Done");
            CollectionOff();
        }

		if ( Input.GetButtonDown("Submit"))
        {
            Debug.Log("Scanning Start");
            CollectionOn();
        }

		if (Input.GetButtonDown("Fire1"))
		{
			Debug.Log("Scan at " + scanOrigin.transform.position.ToString());
			GetPoints(scanOrigin.transform.position);
		}
			
	}

	public void CollectionOn()
	{
		if (!working)
		{
		working = true;
		period = samplingFreq == 0 ? 999999 : 1f / samplingFreq;
		pts.Clear();
		GenerateDirs();

		fs = new FileStream("./" + outputFilename, FileMode.OpenOrCreate);
		fs.SetLength(0);
		w = new StreamWriter(fs);
		}
	}

	public void CollectionOff()
	{
		if (working)
		{
		working = false;

        // Write into file
        w.Close();
        fs.Close();

        Debug.Log("File Wrote");
		}
	}

	void GetPoints(Vector3 position)
	{
		RaycastHit hitInfo = new RaycastHit();
		int ptCnt = 0;
		foreach (Vector3 dir in dirs)
		{
			Ray r = new Ray(position, dir);
			Debug.DrawRay(position, dir, Color.red, 20, true);
			if (Physics.Raycast(r , out hitInfo, 20.0f))	
			{
				Vector3 pt = hitInfo.point;
				w.WriteLine("" + pt.x.ToString("f5") + ' ' + pt.z.ToString("f5") + ' ' + pt.y.ToString("f5") + " ");
				++ptCnt;
			}
		}

		Debug.Log("" + ptCnt + " points scanned");
	}
		
	void GenerateDirs()
	{
		dirs.Clear();
		float horStep = 2 * Mathf.PI / horLaser;
		for (int hor = 0; hor < horLaser; ++hor)
		{
			Vector3 horDirBase = new Vector3();
			horDirBase.x = 1f * Mathf.Sin(hor * horStep);
			horDirBase.z = 1f * Mathf.Cos(hor * horStep);
			for (int ver = 0; ver < verLaser; ++ver)
			{
				Vector3 dir = horDirBase;
				dir.y = 1f * Mathf.Tan(0.5f * Mathf.PI * ((float)verViewField / 90) / verLaser * ver / 2);
				dirs.Add(dir);
				dir.y = -dir.y;
				dirs.Add(dir);
			}
		}
        Debug.Log("Direction Generated");
	}
}
