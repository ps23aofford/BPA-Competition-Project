﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Data.SQLite;
using System.IO;
public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    public float level = 1;

    public int gold = 0;
    public int playerClass = 0; // 1 is warrior, 2 is rouge, 3 is mage
    public float playerMaxHealth = 5;
    public float playerHealth = 0;
    public float playerMaxMana = 100;
    public float playerMana = 100;
    public float potions = 0;
    public int MageBasicAttackLevel = 1;
    public GameObject playerWeapon;
    public GameObject player;
    public List<string> mageSpells = new List<string>();
    public List<GameObject> weaponsOwned = new List<GameObject>();
    public List<int> weaponLevels = new List<int>();
    public Image spellBar;
    public Image[] spriteUIImages;
    public GameObject[] AllWeapons = { };
    SQLiteConnection connection;
    public GameObject pauseMenu;
    public GameObject pauseMenuInstance;
    public bool paused = false;


    public float bossHealth = 0;


    public bool playing = false;

    public string firstLevelScenePath = "Assets/levels/Forest/Forest.unity";
    private string secondLevelScenePath = "Assets/levels/Desert/Desert.unity";
    private string thirdLevelScenePath = "Assets/levels/Cave/Cave.unity";
    private string fourthLevelScenePath = "Assets/levels/Collesium/Collesium.unity";
    private string WarriorShopScenePath = "Assets/UI/WarriorBuyMenu.unity";
    private string RogueShopScenePath = "Assets/UI/RougeBuyMenu.unity";
    private string MageShopScenePath = "Assets/UI/mage buy menu 1.unity";
    string databasePath = Application.streamingAssetsPath + "/Saves/saves.db";

    public string loadScenePath = "Assets/UI/LoadingScene.unity";
    public string NextScene = "";
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            StartCoroutine(InitDataBase());
        }

        else if (instance != this)
        {
            instance.spellBar = spellBar;
            instance.spriteUIImages = spriteUIImages;
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);

    }
    void Update()
    {
        if (playerHealth <= 0) {
            playerHealth = 9999;
            ChangeScene("Assets/UI/GameOver.unity");
        }

        if (Input.GetKeyDown(KeyCode.Q) && potions > 0 && level % 1 != .5f && !paused && playerHealth <=20)
        {
            potions--;
            player.GetComponent<PlayerHealth>().usePotion();
        }
        if (Input.GetKeyDown(KeyCode.Escape) && level % 1 != .5f&&playerHealth<=20)
        {
            if (!paused)
                Pause();
            else unPause();
        }
    }
    public void endLevel() {
        level += .5f;
        playerHealth = playerMaxHealth;
        if (level % 1 != 0)
        {
            if (playerClass == 1)
                ChangeScene(WarriorShopScenePath);
            else if (playerClass == 2)
                ChangeScene(RogueShopScenePath);
            else
            {
                ChangeScene(MageShopScenePath);
            }
        }
        else if (level == 1)
        {
            ChangeScene(firstLevelScenePath);
        }
        else if (level == 2)
        {
            ChangeScene(secondLevelScenePath);
        }
        else if (level == 3)
        {
            ChangeScene(thirdLevelScenePath);
        }
        else if (level == 4) {
            ChangeScene(fourthLevelScenePath);
        }
    }
    public void ChangeScene(string scenePath)
    {
        Time.timeScale = 1;
        NextScene = scenePath;
        SceneManager.LoadScene(loadScenePath, LoadSceneMode.Single);
    }
    IEnumerator InitDataBase()
    {


        if (!File.Exists(databasePath))
        {
            File.Create(databasePath);
            yield return new WaitForSeconds(2);
        }
        databasePath = "URI=file:" + databasePath + ";";
        print(databasePath);
        connection = new SQLiteConnection(databasePath);
        connection.Open();
        SQLiteCommand command = connection.CreateCommand();
        command.CommandText = "PRAGMA foreign_key = ON;";
        command.ExecuteNonQuery();
        command = connection.CreateCommand();
        command.CommandText = "CREATE TABLE IF NOT EXISTS playerSaves (File INTEGER PRIMARY KEY, " +
            "Level DECIMAL," +
            "PlayerHealth DECIMAL," +
            "Coins INTEGER," +
            "Potions INTEGER," +
            "Class INTEGER)";
        command.ExecuteNonQuery();
        command.CommandText = "CREATE TABLE IF NOT EXISTS weaponSaves (File INTEGER PRIMARY KEY, " +
            "WeaponOne STRING," +
            "WeaponOneLevel INTEGER," +
            "WeaponTwo STRING," +
            "WeaponTwoLevel INTEGER," +
            "WeaponThree STRING," +
            "WeaponThreeLevel INTEGER)";
        command.ExecuteNonQuery();
        command.CommandText = "CREATE TABLE IF NOT EXISTS spellSaves (File INTEGER PRIMARY KEY, " +
            "SpellOne TEXT," +
            "SpellTwo TEXT," +
            "SpellThree TEXT," +
            "MageAttackLevel INTEGER)";
        command.ExecuteNonQuery();
        connection.Close();
        yield break;
    }
    public void saveFile()
    {
        connection.Open();
        SQLiteCommand command = connection.CreateCommand();
        command.CommandText = string.Format("INSERT or REPLACE INTO playerSaves(File, Level, PlayerHealth, Coins, Potions,Class) " +
            "VALUES({0},{1},{2},{3},{4},{5})", 0, level, playerMaxHealth, gold, potions, playerClass);
        command.ExecuteNonQuery();
        //Saving Each weapon
        if (weaponsOwned.Count == 1)
        {
            Weapon weaponOne = weaponsOwned[0].GetComponent<Weapon>();
            command.CommandText = string.Format("INSERT or REPLACE INTO weaponSaves VALUES({0},'{1}',{2},{3},{4},{5},{6})"
                , 0, weaponOne.name, weaponLevels[0], "NULL", 0, "NULL", 0);
        }
        else if (weaponsOwned.Count == 2)
        {
            Weapon weaponOne = weaponsOwned[0].GetComponent<Weapon>();
            Weapon weaponTwo = weaponsOwned[1].GetComponent<Weapon>();
            command.CommandText = string.Format("INSERT or REPLACE INTO weaponSaves VALUES({0},'{1}',{2},'{3}',{4},{5},{6})"
                , 0, weaponOne.name, weaponLevels[0], weaponTwo.name, weaponLevels[1], "NULL", 0);
        }
        else if (weaponsOwned.Count == 3)
        {
            Weapon weaponOne = weaponsOwned[0].GetComponent<Weapon>();
            Weapon weaponTwo = weaponsOwned[1].GetComponent<Weapon>();
            Weapon weaponThree = weaponsOwned[2].GetComponent<Weapon>();
            command.CommandText = string.Format("INSERT or REPLACE INTO weaponSaves VALUES({0},'{1}',{2},'{3}',{4},'{5}',{6})"
                , 0, weaponOne.name, weaponLevels[0], weaponTwo.name, weaponLevels[1], weaponThree.name, weaponLevels[2]);
        }
        else
            command.CommandText = string.Format("INSERT or REPLACE INTO weaponSaves VALUES({0},{1},{2},{3},{4},{5},{6})"
                , 0, "NULL", 0, "NULL", 0, "NULL", 0);
        command.ExecuteNonQuery();
        //Save Spells
        command.CommandText = string.Format("INSERT or REPLACE INTO spellSaves(File) " +
            "VALUES({0})", 0);
        if (mageSpells.Count == 1)
        {
            command.CommandText = string.Format("INSERT or REPLACE INTO spellSaves VALUES(0,'{0}','{1}','{2}','{3}')",
                mageSpells[0], "NULL", "NULL", MageBasicAttackLevel);
        }
        else if (mageSpells.Count == 2)
        {
            command.CommandText = string.Format("INSERT or REPLACE INTO spellSaves VALUES(0,'{0}','{1}','{2}','{3}')",
                mageSpells[0], mageSpells[1], "NULL", MageBasicAttackLevel);
        }
        else if (mageSpells.Count == 3)
        {
            command.CommandText = string.Format("INSERT or REPLACE INTO spellSaves VALUES(0,'{0}','{1}','{2}','{3}')",
                mageSpells[0], mageSpells[1], mageSpells[2], MageBasicAttackLevel);
        }
        else
            command.CommandText = string.Format("INSERT or REPLACE INTO spellSaves VALUES(0,'{0}','{1}','{2}','{3}')",
                "NULL", "NULL", "NULL", MageBasicAttackLevel);
        command.ExecuteNonQuery();
        connection.Close();

    }
    public void loadFile()
    {
        connection.Open();
        SQLiteCommand readCommand = connection.CreateCommand();
        SQLiteDataReader reader;
        string query = "SELECT * FROM playerSaves";
        readCommand.CommandText = query;
        reader = readCommand.ExecuteReader();
        while (reader.Read())
        {
            level = float.Parse(reader[1].ToString());
            playerMaxHealth = float.Parse(reader[2].ToString());
            gold = int.Parse(reader[3].ToString());
            potions = int.Parse(reader[4].ToString());
            playerClass = int.Parse(reader[5].ToString());
        }
        reader.Close();
        query = "SELECT * FROM weaponSaves";
        readCommand.CommandText = query;
        reader = readCommand.ExecuteReader();
        weaponsOwned.Clear();
        int Itemindex = 0;
        while (reader.Read())
        {
            for (int e = 1; e <= 5; e += 2) {
                for (int i = 0; i < AllWeapons.Length; i++)
                {
                    string weaponName = AllWeapons[i].GetComponent<Weapon>().name;
                    if (weaponName == reader[e].ToString())
                    {
                        weaponsOwned.Add(AllWeapons[i]);
                        weaponLevels[Itemindex] = (int.Parse(reader[e + 1].ToString()));
                        Itemindex++;
                    }
                }
            }
        }
        reader.Close();
        query = "SELECT * FROM spellSaves";
        readCommand.CommandText = query;
        reader = readCommand.ExecuteReader();
        mageSpells.Clear();
        while (reader.Read())
        {
            mageSpells.Add(reader[1].ToString());
            mageSpells.Add(reader[2].ToString());
            mageSpells.Add(reader[3].ToString());
            MageBasicAttackLevel = int.Parse(reader[4].ToString());
        }
        reader.Close();
        connection.Close();
        loadLevel();
        playerHealth = playerMaxHealth;
        playerWeapon = weaponsOwned[0];
    }
    public void loadLevel() {
        if (level % 1 != 0)
        {
            if (playerClass == 1)
                ChangeScene(WarriorShopScenePath);
            else if (playerClass == 2)
                ChangeScene(RogueShopScenePath);
            else
            {
                ChangeScene(MageShopScenePath);
            }
        }
        else if (level == 1) {
            ChangeScene(firstLevelScenePath);
        }
        else if (level == 2)
        {
            ChangeScene(secondLevelScenePath);
        }
        else if (level == 3)
        {
            ChangeScene(thirdLevelScenePath);
        }
        else if (level == 4)
        {
            ChangeScene(fourthLevelScenePath);
        }
    }
    public void saveAndQuit() {
        saveFile();
        Quit();
    }
    public void Quit() {
        Application.Quit();
    }
    public void nextLevel() {
        level += .5f;
    }
    public void Pause() {
        pauseMenuInstance = (GameObject)Instantiate(pauseMenu, Vector3.zero, Quaternion.identity);
        Time.timeScale = 0;
        paused = true;
    }
    public void unPause() {
        pauseMenuInstance.GetComponent<PauseMenu>().kill();
        Time.timeScale = 1;
        paused = false;
    }
}