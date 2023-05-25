using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UIElements;
using Thirdweb;
using TMPro;

public class PlayerController : NetworkBehaviour
{
    private Rigidbody playerRB;
    public float jumpForce;

    public float lateralForce;
    public float gravityModifier;

    public bool isOnGround = true;

    private float player_name_val;

    [SerializeField] Camera mainCam;

    [SerializeField] Vector3 offset;

    [SerializeField] Animator anim;

    [SerializeField] TextMeshPro username;

    [SerializeField] int collisionPenalty = 50;

    [SerializeField] int speedBoost1 = 500;

    [SerializeField] float boostIntensity = 1.25f;

    [SerializeField] int boostLength = 1;

    [SerializeField]
    float fallPenalty = 50f;

    float speed = 0.0f;

    [SerializeField] float raceSpeed = 50.0f;

    VisualElement root;

    GameObject UIObject;

    Label gameMessage;

    Label playerCount;

    Label countDownLabel;

    /*
    [SyncVar(hook = nameof(onPlayerCountChange))]
    int serverCount = 0;
    */

    public bool isRaceActive = false;

    SharedData shared;

    MyNetworkManager _nm;

    public int serverCounterLength = 15;

    private Player playerInput;

    string DROP_ERC20_CONTRACT = "0xA112614CB7262336E9AC667fa0b7233FD3E041F7";

    public TMP_Text balanceText;

    Prefab_ConnectWallet wallet_prefab;

    public string dancingID;

    public string emoteID = "-1";

    bool isFootstepsPlaying = false;

    private void Update()
    {
        //Debug.Log("isRaceActive: " + isRaceActive);
        if (!isLocalPlayer) return;

        Vector2 movementInput = playerInput.PlayerMain.Move.ReadValue<Vector2>();
        move(movementInput);


        if (shared.getIsRacing() && speed == 0)
        {
            go();
        }
        //Debug.Log(movementInput);
    }

    public void move(Vector2 input)
    {
        transform.Translate(Vector3.right * Time.deltaTime * input.x * lateralForce);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        shared = GameObject.FindGameObjectWithTag("shared").GetComponent<SharedData>();

    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        UIObject = GameObject.FindGameObjectWithTag("UIDoc");
        root = UIObject.GetComponent<UIDocument>().rootVisualElement;
        playerRB = GetComponent<Rigidbody>();
        wallet_prefab = GameObject.FindGameObjectWithTag("wallet").GetComponent<Prefab_ConnectWallet>();

        //container.visible = false;

        // Configure restart and go buttons
        /*
        Button buttonLeft = root.Q<VisualElement>("button-container").Q<Button>("left");
        buttonLeft.clicked += () => moveLeft();

        Button buttonRight = root.Q<VisualElement>("button-container").Q<Button>("right");
        buttonRight.clicked += () => moveRight();

        Button buttonJump = root.Q<VisualElement>("button-container").Q<Button>("jump");
        buttonJump.clicked += () => moveJump();

        Button buttonGo = root.Q<VisualElement>("button-container").Q<Button>("go");
        buttonGo.clicked += () => { go(); };
        */


        //VisualElement container = root.Q<VisualElement>("container");
        //root.Q<VisualElement>("container").visible = false;
        gameMessage = root.Q<VisualElement>("message-container").Q<Label>("message");

        shared = GameObject.FindGameObjectWithTag("shared").GetComponent<SharedData>();
    }

    public override void OnStopClient()
    {
        //shared = GameObject.FindGameObjectWithTag("shared").GetComponent<SharedData>();
        //Debug.Log("OnStopClient " + shared.GetSharedValue());
        //CmdNewPlayerDisconnect();
        //Handle on client left
        if (isServer)
        {
            Debug.Log("isServer: OnStopClient");
        }
        base.OnStopClient();
    }

    // Start is called before the first frame update


    void Start()
    {
        if (isServer)
        {
            // Example of spawning object on server
            /*
            Debug.Log("Server Started");
            GameObject finishLine = Instantiate(finishPrefab, finishPosition, finishRotation);
            NetworkServer.Spawn(finishLine);
            */
        }

        if (!isLocalPlayer)
        {
            // Disable cam if not local player
            mainCam.gameObject.SetActive(false);
            return;
        }

        if (isLocalPlayer)
        {
            playerInput = new Player();
            playerInput.Enable();

        }

        player_name_val = Random.value;
        Debug.Log("My Value " + player_name_val);

        Physics.gravity *= gravityModifier;
        playerCount = root.Q<VisualElement>("player-count-container").Q<Label>("players");
        countDownLabel = root.Q<VisualElement>("container").Q<Label>("countdown");


        //wallet_prefab.GetERC1155();

        // Get Dance ID here
        dancingID = "isDancing";

        // Get 1155s
    }

    private void LateUpdate()
    {

        if (isServer)
        {
            /*
            int _playCount = shared.GetPlayerCount();
            bool rState = shared.getIsRacing();
            Debug.Log($"pCount: {_playCount} - rState: {rState}");
            */

            // If more then 2 players and not racing start clock and not counting already
            //Debug.Log($"isCounting: {shared.GetIsCounting()}");
            if (shared.GetPlayerCount() > 1 && !shared.getIsRacing() && !shared.GetIsCounting())
            {
                //Debug.Log("STARTING RACE");
                shared.SetIsCounting(true);
                StartCoroutine(shared.updateCountdown(serverCounterLength));
            }

            //If less than 2 players and racing set race state to false
            if (shared.GetPlayerCount() < 2)
            {
                shared.setRaceState(false);
                shared.SetIsCounting(false);
            }
        }

        if (!isLocalPlayer) return; // local player only after this line

        if (playerInput.PlayerMain.Jump.triggered && isOnGround)
        {
            moveJump();
        }

        mainCam.transform.position = transform.position + offset;

        try
        {
            playerCount.text = shared.GetPlayerCount().ToString();
        }
        catch (System.Exception)
        {

            Debug.Log("nO COUNNTERS");
        }


        if (shared.GetCountDown() == 6)
        {
            resetPlayer();
            anim.SetBool(dancingID, false);
            anim.SetBool("isGo", false);
            if (emoteID == "-1")
            {
                GetRandomERC1155();
            }
            stopFootsteps();
            /*
            if (username.text == "")
            {
                username.text = wallet_prefab.getWalletDisplay(); // change this to only happen once in the app
            }
            */
        }

        if (shared.GetCountDown() < 1)
        {
            countDownLabel.text = "";
        }
        else
        {
            countDownLabel.text = shared.GetCountDown().ToString();
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!isLocalPlayer) return;

        /*
        if (Input.GetKeyDown(KeyCode.Space) && isOnGround)
        {
            //anim.SetBool("isJumping", true);
            //playerRB.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            //isOnGround = false;
            StartCoroutine(moveJump());
        }*/


        transform.Translate(Vector3.forward * Time.deltaTime * speed * Random.value);
    }

    private void OnCollisionEnter(Collision other)
    {

        //if (!isLocalPlayer) return;

        if (other.gameObject.CompareTag("ground") && isLocalPlayer)
        {
            anim.SetBool("isJumping", false);
            isOnGround = true;
            // Probably should only do 
            if (shared.getIsRacing() && !isFootstepsPlaying)
            {
                playFootsteps();
                //Debug.Log("Playing footsteps");
            }

        }

        else if (other.gameObject.name == "FinishLine") // check if anyone crossed
        {
            //Debug.Log("FINISH LINE (isRaceActive) (isLocalPlayer)" + isRaceActive + ": " + isLocalPlayer);

            if (shared.getIsRacing())
            {
                shared.setRaceState(false);

                if (isLocalPlayer)
                {
                    anim.SetBool(dancingID, true);
                    Debug.Log("Winner");
                    wallet_prefab.ClaimERC20("5");
                    GameObject.FindGameObjectWithTag("winaudio").GetComponent<AudioSource>().Play();
                }
            }
            else
            {
                if (isLocalPlayer)
                {
                    anim.SetBool("isGo", false);
                    Debug.Log("Loser");
                    GameObject.FindGameObjectWithTag("lossaudio").GetComponent<AudioSource>().Play();
                }
            }

            if (isLocalPlayer)
            {
                speed = 0;
            }
        }

        if (!isLocalPlayer) return;

        if (other.gameObject.CompareTag("box"))
        {
            //Debug.Log("BOX");
            playerRB.AddForce(Vector3.forward * -collisionPenalty, ForceMode.Impulse);
            GameObject.FindGameObjectWithTag("boxaudio").GetComponent<AudioSource>().Play();

        }

        if (other.gameObject.CompareTag("boost"))
        {
            Debug.Log("boost");
            StartCoroutine(speedBoost(boostLength, boostIntensity));
            GameObject.FindGameObjectWithTag("powerup").GetComponent<AudioSource>().Play();

            //playerRB.AddForce(Vector3.forward * speedBoost1, ForceMode.Impulse);
            //speed *= 2;
        }

        if (other.gameObject.CompareTag("water"))
        {
            GameObject.FindGameObjectWithTag("boxaudio").GetComponent<AudioSource>().Play();
            float newPosition = transform.position.z - fallPenalty;
            if (newPosition > 0)
            {
                transform.position = new Vector3(0f, 1f, transform.position.z - fallPenalty);
            }
            else
            {
                transform.position = new Vector3(0f, 1f, 0f);
            }

        }

        if (other.gameObject.CompareTag("wallL"))
        {
            playerRB.AddForce(Vector3.right * speedBoost1, ForceMode.Impulse);
        }

        if (other.gameObject.CompareTag("wallR"))
        {
            playerRB.AddForce(Vector3.left * speedBoost1, ForceMode.Impulse);
        }
    }

    public void go()
    {
        //Debug.Log("Go");
        playFootsteps();
        if (shared.getIsRacing())
        {
            speed = raceSpeed;
            anim.SetBool("isGo", true);

            if (isLocalPlayer)
            {
                anim.SetBool("isGo", true);
            }

            //Debug.Log("GO");
            //root.Q<VisualElement>("container").visible = false;
        }
    }

    void moveJump()
    {
        //Debug.Log("Jump");
        stopFootsteps();
        anim.SetBool("isJumping", true);
        playerRB.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isOnGround = false;
    }

    public void resetPlayer()
    {
        //Debug.Log("reset position");
        speed = 0;
        transform.position = new Vector3(transform.position.x, 1.0f, 0.0f);
    }


    IEnumerator speedBoost(int length, float intensity)
    {
        // Boost speed
        playerRB.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        speed *= intensity;
        yield return new WaitForSeconds(length);
        speed /= intensity;
        // return speed
    }

    public async void GetRandomERC1155()
    {
        bool isConnected = await ThirdwebManager.Instance.SDK.wallet.IsConnected();
        if (!isConnected) return; // return if wallet not connected

        const string ERC1155_CONTRACT = "0x59336Fd357f07a6501B2444556ae98633B741297";
        Contract contractERC1155 = ThirdwebManager.Instance.SDK.GetContract(ERC1155_CONTRACT);
        List<NFT> tempNFTList = await contractERC1155.ERC1155.GetOwned();
        if (tempNFTList.Count > 0)
        {
            NFT randomNFT = GetRandomNFT(tempNFTList);
            emoteID = randomNFT.metadata.id;
            if (emoteID == "-1")
            {
                dancingID = $"isDancing";

            }
            else
            {
                dancingID = $"isDancing{emoteID}";
            }
        }
        else
        {
            print("No emotes.");
        }
        //Console.WriteLine($"random id: {randomNFT.metadata.id} name: {randomNFT.metadata.name}");
    }

    public NFT GetRandomNFT(List<NFT> list)
    {
        print($"Count: {list.Count}");
        int randomIndex = (int)UnityEngine.Random.Range(0, list.Count);
        return list[randomIndex];
    }

    public void playFootsteps()
    {
        GameObject.FindGameObjectWithTag("footsteps").GetComponent<AudioSource>().Play();
        isFootstepsPlaying = true;
    }

    public void stopFootsteps()
    {
        GameObject.FindGameObjectWithTag("footsteps").GetComponent<AudioSource>().Stop();
        isFootstepsPlaying = false;
    }

    /*
    async void claim()
    {
        
        Contract contract = ThirdwebManager.Instance.SDK.GetContract(DROP_ERC20_CONTRACT);
        var result = await contract.ERC20.Claim("5");
        CurrencyValue nativeBalance = await ThirdwebManager.Instance.SDK.wallet.GetBalance(DROP_ERC20_CONTRACT);
        balanceText.text = $"{nativeBalance.value.ToEth()} Bull Tokens";
        
    }

    */


    /********************** Client **************************************/




    /********************** Client **************************************/


    /********************** Server **************************************/

}





/*****************************************************************************************************/



/*

    [ClientRpc]
    void RpcNewPlayerJoined()
    {
        int _newCount = shared.GetSharedValue();
        Debug.Log($"Joined New Count: {_newCount}");
        playerCount.text = _newCount.ToString();
    }

    [ClientRpc]
    void RpcNewPlayerDisconnect()
    {
        int _newCount = shared.GetSharedValue();
        Debug.Log($"Disconnect New Count: {_newCount}");
        playerCount.text = _newCount.ToString();
    }


*/

/*

[Command]
    public void CmdPlayerDisconnect(float id)
    {
        shared = GameObject.FindGameObjectWithTag("shared").GetComponent<SharedData>();
        int _pCount = shared.GetSharedValue();
        //RpcPlayerDisconnect(id, _pCount);
    }


    */

/*
[Command]
public void CmdPlayerJoined(float id)
{
    shared = GameObject.FindGameObjectWithTag("shared").GetComponent<SharedData>();
    int _pCount = shared.GetSharedValue();
    RpcPlayerJoined(id, _pCount);
    Debug.Log("Player Joined " + _pCount);


    //serverCount = serverCount + 1;
    //Debug.Log(" Count " + serverCount);

    /*
    if (isServer)
    {
        mainManager.incrementPlayerCount();
        RpcPlayerJoined(id, 0);
    }
    */
//serverCount++;
//Debug.Log("Server Count: " + serverCount + " " + Random.value);
//int newPlayerCount = int.Parse(playerCount.text) + 1;
//Debug.Log("Player Joined " + id + " " + newPlayerCount);
/*
if (isServer)
{
    mainManager.incrementPlayerCount();
    int _pCount = mainManager.getPCount();

}
// ADD END COMMENT HERE WHEN UNCOMMENING THIS FUNCTION
}*/

/*

[ClientRpc]
public void RpcPlayerDisconnect()
{
    //playerCount.text = pCount.ToString();
    Debug.Log("RpcPlayerDisconnect ");
}

*/

/*
[ClientRpc]
public void RpcPlayerJoined(float id, int pCount)
{

    //Debug.Log("Player Joined " + id + " - " + newPlayerCount.ToString());
    playerCount.text = pCount.ToString();
    if (pCount > 1)
    {
        StartCoroutine(updateCountdown(3));
    }

    //Debug.Log("CLIENTs: Player Joined " + id + " " + pCount);
} 
*/


/*
[Command]
public void CmdfinishRace(float id)
{
    //Debug.Log("CmdfinishRace (isRaceActive) " + isRaceActive);
    // RPC means the server is sending messages to the clients
    if (isRaceActive)
    {
        RpcFinish(id);
        RpcCountDown(3);
        //isRaceActive = false;
    }
} 
*/


/*
IEnumerator updateCountdown(int count)
{
    //yield return new WaitForSeconds(5);
    anim.SetBool("isGo", false);
    anim.SetBool("isDancing", false);
    transform.position = new Vector3(0.0f, 0.5f, 0.0f);

    for (int i = count; i > 0; i--)
    {
        yield return new WaitForSeconds(1);
        Debug.Log("Count down: " + i);
    }
    Debug.Log("GOO");



    //Debug.Log("startRace(): (isRaceActive) " + isRaceActive);
}
*/



/*

[ClientRpc]
public void RpcCountDown(int count)
{
    Debug.Log("Starting countdown.");

    // Use this in a for loop to count down from count until count == 0
    // Also update a flag (maybe with sync var that basically stops the player from going before count = 0)
    StartCoroutine(updateCountdown(count));
}

[ClientRpc]
public void RpcFinish(float playerID)
{
    Debug.Log("Someone Finished (isRaceActive)" + playerID + " - " + isRaceActive);
    anim.SetBool("isDancing", true);
    //isRaceActive = false;
    speed = 0;
    gameMessage.text = playerID + " Won!";
    //playerMsg.text = "Done";
    root.Q<VisualElement>("container").visible = true;
} 
*/



/*
public void resetPlayer()
{
    //Debug.Log("reset position");
    speed = 0;
    transform.position = new Vector3(0.0f, 0.5f, 0.0f);
}
*/





/*
IEnumerator initRace(int waitCount)
{
    Debug.Log("Race count down starts in " + waitCount + " seconds.");
    yield return new WaitForSeconds(waitCount);
    StartCoroutine(updateCountdown(3));
    //RpcCountDown(3);
}
*/




/*


    [Command]
public void serverStartRace()
{
    clientStartRace();
}

[ClientRpc]
public void clientStartRace()
{
    isRaceActive = true;
}

*/

/*
[ServerCallback]
void OnTriggerEnter(Collider other)
{
    //Debug.Log(other.gameObject.name);
    RpcFinish(player_name_val);
    //raceState = true;
}
*/


/*
[ClientRpc]
public void RpcFinish(float playerID)
{
    Debug.Log("Someone Finished " + playerID);
    playerMsg.text = "Done";
    root.Q<VisualElement>("container").visible = true;
}
*/


/*
private void OnCollisionEnter(Collision other)
{
    ///if (!isLocalPlayer) return;

    isOnGround = true;
    Debug.Log(other.gameObject.name);
    if (other.gameObject.name == "Ground")
    {
        anim.SetBool("isJumping", false);
    }
    else if (other.gameObject.name == "Finish")
    {
        // If this is the server send a message to other clients letting them know someone finshed
        CmdfinishRace(player_name_val);
        anim.SetBool("isDancing", true);
        speed = 0;
        //raceState = true;
        //StartCoroutine(prepareForNextRound());
    }
}*/

/*
void SetRaceState(bool oldVal, bool newVal)
{
    Debug.Log(oldVal + " : " + newVal);
    // give user instructions here
}*/

// Command means this is being called from the client to the server

/*


[Command]
public void CmdStartCountDown()
{

}



[ClientRpc]
public void RpcFinish(float playerID)
{
    Debug.Log(playerID);
}


*/
