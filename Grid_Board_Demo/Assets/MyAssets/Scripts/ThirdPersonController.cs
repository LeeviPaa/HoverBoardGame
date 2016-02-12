using UnityEngine;
using System.Collections;


public class ThirdPersonController : MonoBehaviour
{

    public GameObject mesh;
    public GameObject speedMeterFill;
    public float shiftSpeed = 70.0f;
    private float speed = 1.0F;

    public float gravityRot = 10.0f;
    public float acceleration = 5.0f;
    public float deceleration = 0.1f;
    public float HighAngleDecel = 50.0f;
    public float angleAcc = 100.0f;

    public float jumpSpeed = 8.0F;
    public float gravity = 10.0F;
    private Rigidbody prb;
    RaycastHit hit;
    RaycastHit hitFront;
    RaycastHit meshHitDown;
    public float driftSpeedAcc = 20.0f;
    public float driftSpeed = 0.0f;
    public float maxDistance = 100.0f;
    public float Rspeed = 0.1F;
    public float RotSpeed = 5.0f;
    float RotSpeedHolder;
    public float rotAcc = 1.0f;
    public float meshRotSpeed = 10.0f;
    //public float ClipForce = 10.0f;
    //public float slidingForceMultiplier = 50;

    public float boostDepletion = 100.0f;
    public float boostamount = 150.0f;
    Transform previousPos;
    Transform currentPos;
    Quaternion to;
    Quaternion tilt;
    Vector3 speedVector;
    float hitAverage;
    int currFrame = 0;
    float angle;
    bool grounded;

    private float points;

    float pointHolder;
    public float pointMultiplyer = 100.0f;
    void Start()
    {
        prb = GetComponent<Rigidbody>();
        currentPos = transform;
        previousPos = currentPos;
        RotSpeedHolder = RotSpeed;
        

    }
    public void setspeed(float setted)
    {
        speed = setted;
        //setter for the speed variable to other scripts and objects.
    }
    public void setpos(Transform upPos)
    {
        transform.position = upPos.position;
        transform.rotation = upPos.rotation;
    }
    public int getspeed()
    {
        return (int)speed;
    }
    public int getpoints()
    {
        return (int)points;
    }
    public int getTempPoints()
    {
        return (int)pointHolder;
    }
    void Update()
    {
        //Asetetaan muihin metodeihin tarvittavat arvot ja muu alustaminen
        SetupControl();

        Debug.Log(speed);
        Debug.Log("angle is: " + angle);

        //sets the rotation of the character according to the ground normal
        
        if (Physics.Raycast(transform.position, -transform.up, out hit, 3.0f))
        {
            groundNormal();
            
        }
        else if(!Physics.Raycast(transform.position, -transform.up, out hit, 3.0f))
        {
            grounded = false;
        }

        //player wasd rotations
        if (Input.GetAxis("Horizontal") != 0 && grounded)
        {
            WasdInputRotate();
            if (RotSpeed <= 80.0f)
            {
                RotSpeed += rotAcc * Time.deltaTime;
            }

        }
        else
        {
            RotSpeed = RotSpeedHolder;
            driftSpeed = 0;
        }

        //Air behaviour control
        if(!grounded )
        {
            AirBehaviour();
            TrickPointControl();
        }

        //
        //tehtävä: lisätään kiihtyvyys kaikkiin nopeuksiin
        //

        //player jumps and shifts etc.
        InputControl();
        //player acceleration
        SpeedControl();
        //downhill acceleration
        DownhillAcceleration();

        //The most important movement code!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        //translates character forward towards the local vector.z (front of the player)
        //ampuu tarkistusraycastin eteenpäin seuraavaan sijaintiin (speed) ja tarkistaa onko osuma lähempänä kuin seuraava sijainti, jos on niin move torward kyseistä pistettä
        if (Physics.Raycast(transform.position, transform.forward, out hitFront, speed*Time.deltaTime))
        {
            if (hitFront.distance < speed * Time.deltaTime && Mathf.DeltaAngle(hitFront.normal.y, Vector3.up.y) > 0.95f)
            {
                transform.position = Vector3.MoveTowards(transform.position, hitFront.point, (speed * Time.deltaTime / 2));
                speed = 0;
            }
        }

        else transform.Translate(driftSpeed * Time.deltaTime, 0, speed * Time.deltaTime);
        //Jos hahmo osuu kohtisuoraan seinään niin scripti pysäyttää hahmon ja asettaa sijainniksi edellisen PreviousPos.positionin
        ClippingPrevention();
    }

    void SetupControl()
    {
        currentPos = transform;
        //Lasketaan raycastit ja hahmon kulmavektorin kulma
        Physics.Raycast(transform.position, -transform.up, out hit, maxDistance);
        Physics.Raycast(transform.position, transform.forward, out hitFront, 10.0f);
        Physics.Raycast(transform.position, -mesh.transform.up, out meshHitDown, 10.0f);
        angle = Vector3.Angle(transform.forward, Vector3.up);
        hitAverage = ((hit.distance + hitFront.distance) / 2);
    }
    void groundNormal()
    {
        grounded = true;
        FaceplantControl();
        if (Physics.Raycast(transform.position, transform.forward, out hitFront, 3.0f))
        {
            to = Quaternion.FromToRotation(transform.up, hit.normal + hitFront.normal) * transform.rotation;
        }
        else if (hit.distance < 3.0f)
        {
            to = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
        }
        transform.rotation = Quaternion.Lerp(transform.rotation, to, Rspeed);
        mesh.transform.rotation = transform.rotation;
    }
    void WasdInputRotate()
    {
        if (Input.GetButton("Left") && speed >= 20)
        {
            //vasemmalle
            tilt = Quaternion.FromToRotation(transform.up, (transform.up - transform.right)) * transform.rotation;
            mesh.transform.rotation = Quaternion.Lerp(transform.rotation, tilt, 0.6f);
            driftSpeed = 3;
        }
        //using getAxis makes the turning somewhat snappy because it takes time to return to normal state and to go up.
        if (Input.GetButton("Right") && speed >= 20)
        {
            //oikealle
            tilt = Quaternion.FromToRotation(transform.up, (transform.up + transform.right)) * transform.rotation;
            mesh.transform.rotation = Quaternion.Lerp(transform.rotation, tilt, 0.6f);
            driftSpeed = -3;
        }
        
        transform.Rotate(0, Input.GetAxis("Horizontal") * RotSpeed * Time.deltaTime, 0, Space.Self);

        //decelerate if player is turning
        speed *= (1 - (deceleration));
    }
    void AirBehaviour()
    {
        if (Input.GetButton("Fire1"))
        {
            //kun pelaaja pitää ctrl pohjassa niin hahmoa voi pyörittää.
        }
        //pelaaja voi kääntää hahmoa ilmassa. Tod.näk korvataan siten, että pelaaja ei käännä fyysistä hahmoa vaan hahmon meshiä, johtuen transform.translate metodin toiminnasta
        //
        transform.Rotate(0, Input.GetAxis("Horizontal") * RotSpeed * Time.deltaTime, 0, Space.Self);
        transform.position += (-Vector3.up*gravity*Time.deltaTime);
        //kääntää hahmon meshiä nuolinäppäimillä!
        mesh.transform.Rotate(Input.GetAxis("VerticalArrow") * (meshRotSpeed * Time.deltaTime), Input.GetAxis("HorizontalArrow") * (meshRotSpeed * 1.0f * Time.deltaTime),0);

        //seuraava if-lause kääntää hahmoa pystysuunnassa alas päin jos hahmo on ilmassa, käytä jos tarpeen.
        if (angle <= 140 && angle >= 30)
        {
            transform.Rotate(gravityRot * Time.deltaTime, 0, 0);
        }
    }
    void TrickPointControl()
    {
        if (Input.GetAxis("VerticalArrow") != 0)
        {
            pointHolder += pointMultiplyer * 1.5f * Time.deltaTime;
        }
        if (Input.GetAxis("HorizontalArrow") != 0)
        {
            pointHolder += pointMultiplyer * Time.deltaTime;
        }

    }
    void DownhillAcceleration()
    {
        if (angle > 92 && speed <= 150)
        {
            speed += (((angle - 90) * angleAcc * Time.deltaTime));
        }
        if (angle < 88 && speed >= -30)
        {
            //kulmanopeus on negatiivinen eli hahmo liikkuu taakse päin. if lauseen sisältö kääntää hahmon ympäri ja muuttaa nopeuden positiiviseksi
            if (angle < 60 && speed <= -1)
            {
                speed = Mathf.Abs(speed);
                transform.Rotate(0, 180, 0);
            }
            else
                speed -= (((90 - angle) * angleAcc * Time.deltaTime));
        }
    }
    void ClippingPrevention()
    {
        //keep character above the surface
        if (hit.distance < 2.0f && grounded)
        {
            transform.Translate(0, gravity * Time.deltaTime, 0);
        }
        if (hit.distance < 1.5f && grounded)
        {
            transform.Translate(0, 10 * Time.deltaTime, 0);
        }
        if (hit.distance < 1.0f && grounded)
        {
            transform.Translate(0, 10 * Time.deltaTime, 0);
        }
        
        if (Physics.Raycast(transform.position, transform.forward, out hitFront, 100.0f))
        {
            if (hitFront.distance < 0.75f)
            {

                speed = 40;
                transform.position = previousPos.position;
            }
        }
        currFrame++;
        if (currFrame >= 60)
        {
            //asettaa previous positionin joka 60. frame.
            previousPos = currentPos;
            currFrame = 0;
        }
    }
    void SpeedControl()
    {
        //ongelma, hyvin pienillä nopeuksilla hahmo räjähtää eteenpäin nappia painettaessa
        if (speed <= 80 && Input.GetAxis("Vertical") > 0 && grounded)
        {
            speed += (acceleration * Time.deltaTime);
        }
        if (speed > -20 && Input.GetAxis("Vertical") < 0 && grounded)
        {
            speed -= ((acceleration * Time.deltaTime) / 2);
        }

        //halt and deceleration if no player input
        if (Input.GetAxis("Vertical") == 0)
        {
            speed *= (1 - deceleration);
        }
    }
    void InputControl()
    {
        //transform position - movement script. Acceleration and deceleration.
        if (Input.GetButton("Jump") && grounded)
        {
            mesh.transform.position = transform.position + new Vector3(0, -0.5f, 0);
            if (Input.GetButtonDown("Jump"))
            {
                //transform.Translate(new Vector3(0, jumpSpeed, 0));
            }
        }
        if (Input.GetButtonUp("Jump") && grounded)
        {
            mesh.transform.position = transform.position + new Vector3(0, 0.5f, 0);
            transform.Translate(new Vector3(0, jumpSpeed, 0));
        }

        speedMeterFill.transform.localScale = new Vector3(1,(1.2f-(boostamount / 100)),1);

        if (Input.GetButton("Fire3") && boostamount > 0) boostamount -= boostDepletion * Time.deltaTime;
        else if(boostamount < 120 )
        {
            //boostin generointi pitää keksiä jotenkin
            //boostamount += boostDepletion * Time.deltaTime;
            //boostia  saa lisää onnistuneista tempuista. Tämä asetetaan faceplant control metodissa
        }
        
        if (Input.GetButton("Fire3") && speed <= shiftSpeed && boostamount > 0)
        {
            
            speed += acceleration * Time.deltaTime;
        }
    }
    void FaceplantControl()
    {
        
        if(!Physics.Raycast(transform.position, -mesh.transform.up, out meshHitDown, 6.5f) && grounded)
        {
            speed = 0;
            pointHolder = 0.0f;
            
        }
        //boostia saa vain tempun jälkeen osuu maahan onnistuneesti. Tämä tarkistetaan tässä metodissa, joten boost asetetaan tässä metodissa.
        if(boostamount < 120)
        {
            boostamount += (pointHolder / 10);
        }
        //jos boostia on liikaa, asetetaan sille arvo 120
        if (boostamount > 120) boostamount = 120;

        points += pointHolder;
        pointHolder = 0;
    }
}
