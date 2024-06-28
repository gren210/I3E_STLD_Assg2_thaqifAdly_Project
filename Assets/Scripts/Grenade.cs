using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Grenade : Interactable
{
    public string interactText;

    [SerializeField]
    float grenadeDistance;

    public float delay = 3f;

    float timer;

    public ParticleSystem effect;

    public float explodeRadius = 50f;

    public float explodeDamage = 70f;

    [HideInInspector]
    public bool thrown = false;

    bool set = true;

    bool exploded = false;

    [SerializeField]
    AudioClip grenadeSound;

    [SerializeField]
    float shakeTimer;

    float shakeTimerStart;

    [SerializeField]
    float shakeIntensity;

    [SerializeField]
    float shakeFrequency;

    [SerializeField]
    float swapThrowForce;

    [SerializeField]
    int iconIndex;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    private void GrenadeExplode()
    {
        //Instantiate(effect, transform.position, transform.rotation);
        effect.GetComponent<ParticleSystem>().Play();

        AudioSource.PlayClipAtPoint(grenadeSound, transform.position);

        Collider[] entities = Physics.OverlapSphere(transform.position, explodeRadius);

        foreach (Collider collider in entities) 
        { 
            if (collider.gameObject.tag == "Enemy")
            {
                Enemy enemy = collider.gameObject.GetComponent<Enemy>();
                collider.gameObject.GetComponent<Enemy>().currentEnemyHealth -= explodeDamage;
                enemy.healthBarGreen.fillAmount = enemy.currentEnemyHealth / enemy.enemyHealth;
            }
            if (collider.gameObject.tag == "Boss")
            {
                Boss enemy = collider.gameObject.GetComponent<Boss>();
                enemy.enemyHealth -= explodeDamage;
                //enemy.healthBarGreen.fillAmount = enemy.EnemyHealth / enemy.enemyHealth;
            }
            else if (collider.gameObject.tag == "Player")
            {
                GameManager.instance.playerHealth -= explodeDamage;
                GameManager.instance.UpdateHealth();
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        timer = delay;
    }

    // Update is called once per frame
    void Update()
    {
        if (thrown)
        {
            if (set)
            {
                GameManager.instance.throwSound.Play();
                GameManager.instance.currentGrenadeIcon.SetActive(false);
                GameManager.instance.currentGrenadeIcon = null;
                gameObject.transform.parent = null;
                gameObject.GetComponent<SphereCollider>().enabled = true;
                gameObject.GetComponent<Rigidbody>().isKinematic = false;
                gameObject.GetComponent<Rigidbody>().AddForce(grenadeDistance * GameManager.instance.playerCamera.transform.forward);
                GameManager.instance.currentGrenade = null;
                GameManager.instance.currentEquippable = null;
                set = false;
            }
            timer -= Time.deltaTime;
            if (timer < 0)
            {
                if (!exploded)
                {
                    GrenadeExplode();
                    exploded = true;
                    gameObject.GetComponent<Rigidbody>().isKinematic = true;
                    gameObject.GetComponent<MeshFilter>().mesh = null;
                    Destroy(gameObject,2f);
                    Destroy(effect, 2f);
                    ShakeCamera(shakeIntensity, shakeFrequency);
                    shakeTimerStart = shakeTimer;

                }

            }
        }
        if (shakeTimerStart > 0)
        {
            shakeTimerStart -= Time.deltaTime;
            if (shakeTimerStart <= 0f)
            {
                ShakeCamera(0f, 0f);
            }
        }
    }

    public override void Interact(Player thePlayer)
    {
        base.Interact(thePlayer);
        if (GameManager.instance.currentGrenade != null)
        {
            if (GameManager.instance.currentGrenade.activeSelf == false)
            {
                GameManager.instance.currentGrenade.SetActive(true);
            }
            GameManager.instance.currentGrenade.transform.parent = null;
            GameManager.instance.currentGrenade.GetComponent<SphereCollider>().enabled = true;
            GameManager.instance.currentGrenade.GetComponent<Rigidbody>().isKinematic = false;
            GameManager.instance.currentGrenade.GetComponent<Rigidbody>().AddForce(swapThrowForce * GameManager.instance.playerCamera.transform.forward);
        }
        if (GameManager.instance.currentEquippable != null && GameManager.instance.currentEquippable != GameManager.instance.currentGrenade)
        {
            GameManager.instance.currentEquippable.SetActive(false);
        }
        gameObject.transform.SetParent(GameManager.instance.playerCamera.transform, false);
        gameObject.GetComponent<SphereCollider>().enabled = false;
        gameObject.GetComponent<Rigidbody>().isKinematic = true;
        GameManager.instance.currentGrenade = gameObject;
        GameManager.instance.currentEquippable = gameObject;
        GameManager.instance.currentEquippable.SetActive(true);
        gameObject.transform.position = GameManager.instance.equipPosition.transform.position;
        gameObject.transform.eulerAngles = GameManager.instance.equipPosition.transform.eulerAngles;
        GameManager.instance.IconSwitchGrenade(iconIndex);
        GameManager.instance.swapItemSound.Play();
    }

    //public override void ShakeCamera(float shakeIntensity, float shakeFrequency)
    //{
        //base.ShakeCamera(shakeIntensity,shakeFrequency);
    //}
}
