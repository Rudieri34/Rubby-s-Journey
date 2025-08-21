using Cysharp.Threading.Tasks;
using InnominatumDigital.Base;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;


public class DialogManager : SingletonBase<DialogManager>
{

    public bool CancelDialog;

    public string LanguageSel = "ENGLISH";

    [SerializeField] private TMP_Text _gameDialog;


    public string GetFilePath( string jsonFileName)
    {
#if UNITY_EDITOR
        return Path.Combine(Application.dataPath, "StreamingAssets", jsonFileName);
#else
        return Path.Combine(Application.streamingAssetsPath, jsonFileName);
#endif
    }

    public async Task SetDialog(string text)
    {

        _gameDialog.text = "";
        _gameDialog.gameObject.SetActive(true);

        bool skipTyping = false;

        for (int i = 0; i < text.Length; i++)
        {
            if (Input.anyKeyDown && i > 3)
            {
                skipTyping = true;
            }

            if (skipTyping)
            {
                _gameDialog.text = text;
                break;
            }

            if (CancelDialog)
            {
                _gameDialog.text = "";

                CancelDialog = false;
                break;
            }

            char c = text[i];

            if (!char.IsWhiteSpace(c) && char.IsLetterOrDigit(c) && UnityEngine.Random.Range(0, 2) == 1)
            {
                SoundManager.Instance.PlaySFX("Type", UnityEngine.Random.Range(0.8f, 1.3f));
            }

            _gameDialog.text += c;
            await UniTask.Delay(30);
        }

        await UniTask.Delay(500);
    }
    public void HideDialog()
    {
        _gameDialog.text = "";
        _gameDialog.gameObject.SetActive(false);
    }

}
