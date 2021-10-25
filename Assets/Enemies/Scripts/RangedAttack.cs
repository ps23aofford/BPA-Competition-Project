﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedAttack : MonoBehaviour
{
    public GameObject weapon;
    private FollowPlayer ai;
    private Animator anim;
    private BoxCollider2D hitbox;

    public float cooldown = 5;
    private float attackCooldown = 0;
    void Start()
    {
        ai = GetComponent<FollowPlayer>();
        anim = GetComponent<Animator>();
        hitbox = GetComponent<BoxCollider2D>();
    }
    void Update()
    {
        if (ai.stopped && !ai.attacking && attackCooldown <= 0)
        {
            ai.attacking = true;
            anim.SetTrigger("attack");
        }
        attackCooldown -= Time.deltaTime;

    }
    public void shoot()
    {
        Vector3 rotation = transform.rotation.eulerAngles;
        rotation = new Vector3(rotation.x, rotation.y, rotation.z - 90);
        Vector3 direction = (ai.target.transform.position - transform.position);
        direction.z = 0.0f;
        Vector3 directionNormalized = direction.normalized;
        GameObject p = (GameObject)Instantiate(weapon, transform.position, Quaternion.Euler(rotation));
        EnemyProjectile pscript = p.GetComponent<EnemyProjectile>();
        pscript.updateDirection(new Vector2(directionNormalized.x, directionNormalized.y));
        attackCooldown = cooldown;
        ai.attacking = false;
    }
}