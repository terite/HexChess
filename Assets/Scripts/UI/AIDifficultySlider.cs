using System.Collections;
using System.Collections.Generic;
using Extensions;
using UnityEngine;
using UnityEngine.UI;

public class AIDifficultySlider : MonoBehaviour
{
    [SerializeField] private Slider slider;
    public Slider difficultySlider => slider;
    public int AILevel => (int)slider.value + 1;
}