using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static TeamAlpha.Source.MonoModel;

namespace TeamAlpha.Source
{
    public class ColorPicker : MonoBehaviour
    {
        [Required]
        public AudioSimpleData audioOnClick;
        [Required]
        public Transform cellsHolder;
        [Required, AssetsOnly]
        public ColorPickerCell cellPrefab;
        [Required]
        public GameObject selectorView;
        public float animTime;

        private List<ColorPickerCell> cells = new List<ColorPickerCell>();
        private void Start()
        {

        }
        public void UpdateView()
        {
            cellsHolder.DestroyAllChilds();
            cells.Clear();
            foreach (PartType partType in LevelController.Current.modelPrefab.types)
            {
                if (partType.IsBorder || !LevelController.Current.modelPrefab.IsTypeUsed(partType))
                    continue;
                ColorPickerCell cell = Instantiate(cellPrefab.gameObject, cellsHolder).GetComponent<ColorPickerCell>();
                cell.colorType = partType;
                cell.image.color = partType.material.color;
                cell.button.onClick.AddListener(() => HandleButtonClick(cell));
                cells.Add(cell);
            }
            HandleButtonClick(cells[0]);
        }
        private void HandleButtonClick(ColorPickerCell cellClicked)
        {
            audioOnClick.Play(ProcessorSoundPool.PoolLevel.GameLevel);

            PlayerController.Default.CurType = cellClicked.colorType;
            ProcessorDeferredOperation.Default.Add(() =>
            {
                selectorView.transform.DOKill();
                Vector3 screenPosition = cellClicked.transform.position;
                Vector3 targetLocalPosition = selectorView.transform.parent.InverseTransformPoint(screenPosition);
                selectorView.transform.DOLocalMove(targetLocalPosition, animTime);
            }, true, 1);

            foreach (ColorPickerCell cell in cells)
            {
                cell.transform.DOKill();
                if (cell == cellClicked)
                    cell.transform.DOScale(1f, animTime);
                else
                    cell.transform.DOScale(0.7f, animTime);
            }
        }
    }
}
