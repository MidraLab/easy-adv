using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EasyADV.FungusExtension
{
    public class SetBackgroundImage : MonoBehaviour
    {
        [SerializeField] private List<Sprite> backgroundSprites;

        private Image _image;
        private int _showImageIndex = -1;

        private void Awake()
        {
            _image = GetComponent<Image>();
        }

        public void SetImage(Sprite sprite)
        {
            backgroundSprites.Add(sprite);
        }

        // fungusのコマンドから呼び出す
        public void ShowImage()
        {
            _showImageIndex++;
            _image.sprite = backgroundSprites[_showImageIndex];
        }
    }
}