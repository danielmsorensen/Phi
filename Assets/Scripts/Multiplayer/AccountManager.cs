using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using DatabaseControl;

public class AccountManager : MonoBehaviour {

    [System.Serializable]
    public class Event : UnityEngine.Events.UnityEvent { }

    [Header("Page Management")]
    public Event OnLoginPage;
    public Event OnRegisterPage;
    public Event OnAccountPage;
    public Event OnStartLoading;
    public Event OnStopLoading;
    [Header("Input Fields")]
    public TMP_InputField loginUsernameField;
    public TMP_InputField loginPasswordField;
    [Space]
    public TMP_InputField registerUsernameField;
    public TMP_InputField registerPasswordField;
    public TMP_InputField registerConfirmPasswordField;
    [Space]
    public TMP_InputField loggedInDataInputField;
    public TMP_InputField loggedInDataOutputField;
    [Header("Error Text")]
    public TMP_Text loginErrorText;
    public TMP_Text registerErrorText;
    [Header("Info")]
    public TMP_Text loggedInText;
    
    string username = "";
    string password = "";

    public static string Username;
    public static bool LoggedIn;

    static bool firstStart = true;

    void Awake() {
        ResetUI();
    }

    void Start() {
        if (firstStart) {
            string un = PlayerPrefs.GetString("Username");
            string pw = PlayerPrefs.GetString("Password");

            if (!string.IsNullOrEmpty(un) && un.Length > 3 && !string.IsNullOrEmpty(pw) && pw.Length > 5) {
                username = un;
                password = pw;
                StartLoading();
                StartCoroutine(LoginUser(false));
            }
            else {
                if (OnLoginPage != null) {
                    OnLoginPage.Invoke();
                }
                else {
                    Debug.LogError("Account Error");
                    Application.Quit();
                }
            }
        }
        firstStart = false;
    }

    void ResetUI () {
        loginUsernameField.text = "";
        loginPasswordField.text = "";
        registerUsernameField.text = "";
        registerPasswordField.text = "";
        registerConfirmPasswordField.text = "";
        loggedInDataInputField.text = "";
        loggedInDataOutputField.text = "";
        loginErrorText.text = "";
        registerErrorText.text = "";
        loggedInText.text = "";
    }
    
    IEnumerator LoginUser (bool changePage=true) {
        IEnumerator e = DCF.Login(username, password);
        while (e.MoveNext()) {
            yield return e.Current;
        }
        string response = e.Current as string;

        StopLoading();

        if (response == "Success") {
            ResetUI();
            loginErrorText.text = "Login Successful";
            if (changePage && OnAccountPage != null) {
                OnAccountPage.Invoke();
            }
            loggedInText.text = "Logged In As: " + username;
            LoggedIn = true;
            Username = username;
            PlayerPrefs.SetString("Username", username);
            PlayerPrefs.SetString("Password", password);
        }
        else {
            if (OnLoginPage != null) {
                OnLoginPage.Invoke();
            }
            if (response == "UserError") {
                loginErrorText.text = "The username does not exist";
            }
            else {
                if (response == "PassError") {
                    loginErrorText.text = "The password is incorrect";
                }
                else {
                    loginErrorText.text = "Error logging in";
                }
            }
        }
    }
    IEnumerator RegisterUser(bool changePage = true) {
        IEnumerator e = DCF.RegisterUser(username, password, "Null");
        while (e.MoveNext()) {
            yield return e.Current;
        }
        string response = e.Current as string;

        StopLoading();

        if (response == "Success") {
            ResetUI();
            if (changePage && OnAccountPage != null) {
                OnAccountPage.Invoke();
            }
            loggedInText.text = "Logged In As: " + username;
            LoggedIn = true;
            Username = username;
            PlayerPrefs.SetString("Username", username);
            PlayerPrefs.SetString("Password", password);
        }
        else {
            if (OnRegisterPage != null) {
                OnRegisterPage.Invoke();
            }
            if (response == "UserError") {
                registerErrorText.text = "The username is already taken";
            }
            else {
                loginErrorText.text = "Error registering user";
            }
        }
    }
    IEnumerator GetData (bool changePage = true) {
        IEnumerator e = DCF.GetUserData(username, password);
        while (e.MoveNext()) {
            yield return e.Current;
        }
        string response = e.Current as string;

        StopLoading();

        if (response == "Error") {
            ResetUI();
            username = "";
            password = "";
            if (changePage && OnLoginPage != null) {
                OnLoginPage.Invoke();
            }
            loginErrorText.text = "Error getting user data";
        }
        else {
            if (OnAccountPage != null) {
                OnAccountPage.Invoke();
            }
            loggedInDataOutputField.text = response;
        }
    }
    IEnumerator SetData (string data, bool changePage = true) {
        IEnumerator e = DCF.SetUserData(username, password, data);
        while (e.MoveNext()) {
            yield return e.Current;
        }
        string response = e.Current as string;

        StopLoading();

        if (response == "Success") {
            if (changePage && OnAccountPage != null) {
                OnAccountPage.Invoke();
            }
        }
        else {
            ResetUI();
            username = "";
            password = "";
            if (OnLoginPage != null) {
                OnLoginPage.Invoke();
            }
            loginErrorText.text = "Error setting user data";
        }
    }
    
    public void Login () {
        username = loginUsernameField.text.ToLower();
        password = MD5Hash(loginPasswordField.text);

        if (username.Length > 3) {
            if (password.Length > 5) {
                StartLoading();
                StartCoroutine(LoginUser());
            }
            else {
                loginErrorText.text = "The password is incorrect";
            }
        }
        else {
            loginErrorText.text = "The username does not exist";
        }
    }
    
    public void Register () {
        username = registerUsernameField.text.ToLower();
        password = MD5Hash(registerPasswordField.text);
        string confirmedPassword = MD5Hash(registerConfirmPasswordField.text);
        if (username.Length > 3) {
            if (password.Length > 5)
            {
                if (password == confirmedPassword) {
                    StartLoading();
                    StartCoroutine(RegisterUser());
                }
                else {
                    registerErrorText.text = "Passwords do not match";
                }
            }
            else {
                registerErrorText.text = "Passwords must be at least 6 characters long";
            }
        }
        else {
            registerErrorText.text = "Username must be at least 4 characters long";
        }
    }
    
    public void SetData () {
        StartLoading();
        StartCoroutine(SetData(loggedInDataInputField.text));
    }
    public void LoadData () {
        StartLoading();
        StartCoroutine(GetData());
    }
    public void Logout () {
        ResetUI();
        username = "";
        password = "";
        if (OnLoginPage != null) {
            OnLoginPage.Invoke();
        }
        Username = "";
        LoggedIn = false;
    }

    public void StartLoading() {
        if(OnStartLoading != null) {
            OnStartLoading.Invoke();
        }
    }
    public void StopLoading() {
        if(OnStopLoading != null) {
            OnStopLoading.Invoke();
        }
    }

    public static string MD5Hash(string str) {
        System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
        byte[] bytes = ue.GetBytes(str);

        System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] hashBytes = md5.ComputeHash(bytes);

        string hashString = "";

        for (int i = 0; i < hashBytes.Length; i++) {
            hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
        }

        return hashString.PadLeft(32, '0');
    }
}
