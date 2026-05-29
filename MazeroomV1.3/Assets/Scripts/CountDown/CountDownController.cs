using UnityEngine;
using TMPro;

public class CountDownController : MonoBehaviour
{
    [Header("Timer Configure")]
    [Tooltip("Initial Time")]
    public float tempoInicial = 10f;

    [Header("Reference")]
    public TextMeshProUGUI textTMP;

    public TextMeshProUGUI initialText;

    private float tempoRestante;

    void Start()
    {
        tempoRestante = tempoInicial;

        if (textTMP != null)
        {
            textTMP.text = Mathf.CeilToInt(tempoRestante).ToString();
        }
    }

    void Update()
    {
        if (tempoRestante > 0)
        {
            tempoRestante -= Time.deltaTime;

            if (textTMP != null)
            {
                textTMP.text = Mathf.CeilToInt(tempoRestante).ToString();
            }

            if (tempoRestante <= 0)
            {
                tempoRestante = 0;

                if (textTMP != null)
                {
                    textTMP.gameObject.SetActive(false);
                }                
                
                if (initialText != null)
                {
                    initialText.gameObject.SetActive(false);
                }
            }
        }
    }
}
