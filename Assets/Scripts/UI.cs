using UnityEngine;
using Thirdweb;
using UnityEngine.UI;
using Mirror;

public class UI : MonoBehaviour
{
    PlayerController player;

    [SerializeField] GameObject connectButton;

    [SerializeField] GameObject marketButton;

    [SerializeField] GameObject leaderBoardButton;

    [SerializeField] GameObject mainMenu;

    [SerializeField] GameObject controlsCanvas;

    [SerializeField] GameObject countCanvas;

    [SerializeField] GameObject joyStick;

    [SerializeField] GameObject jumpButton;

    [SerializeField] NetworkManager networkManager;

    [SerializeField] string networkAddress;

    [SerializeField] string leaderBoardURL;

    [SerializeField] string marketplaceURL;

    private void Start()
    {
        //hideCanvas(countCanvas);
        connectButton.GetComponent<Button>().onClick.AddListener(() => OnClick());
        leaderBoardButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            Debug.Log("leader board");
            Application.OpenURL(leaderBoardURL);
        });

        marketButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            Debug.Log("Market button");
            Application.OpenURL(marketplaceURL);
        });
        //mainMenu.active = false;
    }

    public void OnClick()
    {
        ConnectClient();
        //Debug.Log("Clicked");
        hideCanvas(mainMenu);
        showCanvas(controlsCanvas);
        showCanvas(joyStick);
        showCanvas(jumpButton);
        //showCanvas(countCanvas);
        GameObject.FindGameObjectWithTag("levelMusic").GetComponent<AudioSource>().Play();
    }

    public void hideCanvas(GameObject canvas)
    {
        Renderer renderer = canvas.GetComponent<Renderer>();
        canvas.SetActive(false);
        if (renderer != null)
        {
            renderer.enabled = false;
        }
    }

    public void showCanvas(GameObject canvas)
    {
        Renderer renderer = canvas.GetComponent<Renderer>();
        canvas.SetActive(true);
        if (renderer != null)
        {
            renderer.enabled = true;
        }
    }

    public void ConnectClient()
    {
        networkManager.networkAddress = networkAddress;
        networkManager.StartClient();
        //mainMusic.Stop();
    }
}
