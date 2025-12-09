using UnityEngine;

public class Camera : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]private Transform CollegeStudent;
   

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(CollegeStudent.position.x,CollegeStudent.position.y+3,transform.position.z);
    }
}
