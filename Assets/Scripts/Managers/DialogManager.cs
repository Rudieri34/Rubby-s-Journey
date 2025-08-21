using Cysharp.Threading.Tasks;
using DG.Tweening;
using InnominatumDigital.Base;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class DialogManager : SingletonBase<DialogManager>
{

    public bool CancelDialog;

    public string LanguageSel = "ENGLISH";

    [SerializeField] private Image _fadeImage;

    [SerializeField] private TMP_ColorGradient _playerColor;
    [SerializeField] private TMP_ColorGradient _npcColor;
    [SerializeField] private TMP_Text _gameDialog;


    public string GetFilePath(string jsonFileName)
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
    public void SetTextColor(bool isPlayer)
    {
        if (isPlayer)
        {
            _gameDialog.colorGradientPreset = _playerColor;
        }
        else
        {
            _gameDialog.colorGradientPreset = _npcColor;
        }
    }

    public void FadeOut()
    {
        _fadeImage.gameObject.SetActive(true);
        _fadeImage.DOFade(1, 1f);
        SoundManager.Instance.FadeSound();
    }
}
