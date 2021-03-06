﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour {
    Rigidbody rbody;
    public Vector3 mov_dir; // movement direction
    public float mov_force = 100;
    public SpriteRenderer sr;
    public Transform wpn_rotator;

    public Animator playerAnimator;
    public AudioSource audio;
    public AudioClip miniGunStop;
    [System.Serializable]
    public class Weapon
    {
        public string name;
        public SpriteRenderer wpn;
        public GameObject bullet;
        public float time_beween_shots;
        public bool multishot;
        public AudioClip weaponSound;
        [Range(0.1f, 1f)]
        public float volume;
    }

    public Weapon[] weapons;

    private Weapon current_weapon;

    public Transform weapon_container;

    public RectTransform health_fg;
    public Image weapon_img;

    private void Start()
    {
        rbody = GetComponent<Rigidbody>();
        GetComponent<Health>().OnDeath = EstoyMuerto;

        current_weapon = weapons[Random.Range(0, weapons.Length)];
        current_weapon.wpn.gameObject.SetActive(true);

        weapon_img.sprite = current_weapon.wpn.sprite;
    }


    public GameObject OnDeathObject;

    void EstoyMuerto() {
        Debug.Log("estoy muerto");
        if ( OnDeathObject != null ) OnDeathObject.SetActive( true );
    }

    public float r = 100f;
    public float r2;

    public void updatehealth() {
        var sd = health_fg.sizeDelta;
        sd.x = 300f * ( GetComponent<Health>().health / 100f );
        health_fg.sizeDelta = sd;
    }

    private void Update() {
        if (Input.GetAxisRaw("Horizontal") != 0f|| Input.GetAxisRaw("Vertical") != 0f)
        {
            playerAnimator.Play("Quieto", 0);
        }
        else
        {
            playerAnimator.Play("Andando", 0);

        }
        updatehealth();

        // flip the character if aiming left/right
        sr.flipX = PlayerCamera.I.aim.transform.position.x < rbody.position.x;

        var dir = ( PlayerCamera.I.aim.position - rbody.position ).normalized;
        r = Vector3.Angle( dir, Vector3.right );
        r2 = Vector3.Angle( dir, Vector3.forward );
        if ( r2 < 90f ) {
            r = -r;
        }

        wpn_rotator.localRotation = Quaternion.Euler(0, r, 0);

        while (r > 360f) r -= 360f;
        while (r < 0f) r += 360f;

        current_weapon.wpn.flipY = !(r < 90f || r > 270);

        var wpn_rot = Quaternion.Euler(0f, 0f, -r);
        current_weapon.wpn.transform.localRotation = wpn_rot;

        elapsed_since_last_shot += Time.deltaTime;

        if (elapsed_since_last_shot > current_weapon.time_beween_shots)
        {
            if (Input.GetMouseButton(0))
            {
                cam_shake.ShakeIt();
                audio.PlayOneShot(current_weapon.weaponSound,current_weapon.volume);
                elapsed_since_last_shot = 0;

                var aim_dir = Quaternion.Euler(0, r, 0) * Vector3.right;
                var aim_dir_1 = Quaternion.Euler(0, r - 10 + Random.value * 4f - 2f, 0) * Vector3.right;
                var aim_dir_2 = Quaternion.Euler(0, r + 10 + Random.value * 4f - 2f, 0) * Vector3.right;
                var aim_dir_3 = Quaternion.Euler(0, r - 20 + Random.value * 4f - 2f, 0) * Vector3.right;
                var aim_dir_4 = Quaternion.Euler(0, r + 20 + Random.value * 4f - 2f, 0) * Vector3.right;
                var n_bullets = current_weapon.multishot ? 8 : 1;

                for (int i = 0; i < n_bullets; i++)
                {
                    //Debug.DrawRay( sr_wpn.transform.position, aim_dir * 1, Color.cyan , 10f );
                    var bullet = Instantiate(current_weapon.bullet, current_weapon.wpn.transform.position, Quaternion.identity);
                    var d = aim_dir;
                    if (i == 1) d = aim_dir_1;
                    else if (i == 2) d = aim_dir_2;
                    else if (i == 3) d = aim_dir_3;
                    else if (i == 4) d = aim_dir_4;
                    bullet.transform.LookAt(current_weapon.wpn.transform.position + d);
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            cam_shake.shaking = false;
            if(current_weapon.name == "Minigun")
            {
                audio.PlayOneShot(miniGunStop, current_weapon.volume);
            }
        }
    }

    public CameraShake cam_shake;
    public float elapsed_since_last_shot = 0;


    private void FixedUpdate() {
        var old_mov_dir = mov_dir;
        mov_dir.x = Input.GetAxis("Horizontal");
        mov_dir.z = Input.GetAxis("Vertical");
        if ( mov_dir.magnitude > 0.5 ) mov_dir.Normalize();
        rbody.AddForce( mov_dir * mov_force );
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "Enemy")
        {
            int damage = collision.gameObject.GetComponent<damageScript>().damageInt;
            GetComponent<Health>().SetDamage(damage);
            collision.gameObject.GetComponent<Rigidbody>().AddForce(-collision.transform.forward * 5000);
        }
    }

    public void ChangeWeapon()
    {
        current_weapon.wpn.gameObject.SetActive(false);
        current_weapon = weapons[Random.Range(0, weapons.Length - 1)];
        current_weapon.wpn.gameObject.SetActive(true);
    }
}


