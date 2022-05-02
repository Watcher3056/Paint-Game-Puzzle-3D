using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static TeamAlpha.Source.LayerDefault;

namespace TeamAlpha.Source
{
    public class PanelLevelsTab : MonoBehaviour
    {
        [Required]
        public Button button;
        [Required]
        public Image imageBG;
        public int fakeLevelsAmount;

        [NonSerialized]
        public List<PanelLevelsCell> cells = new List<PanelLevelsCell>();
        [NonSerialized]
        public List<PanelLevelsCell> fakeCells = new List<PanelLevelsCell>();

        public int PagesCount
        {
            get
            {
                int result = (cells.Count + fakeCells.Count) / PanelLevels.ItemsPerPage;
                if (cells.Count % PanelLevels.ItemsPerPage > 0)
                    result++;

                return result;
            }
        }

        public PanelLevelsCell AddLevel(Level level, bool fake = false, bool fakeLegendary = false)
        {
            PanelLevelsCell cell =
                Instantiate(PanelLevels.Default.prefabCell.gameObject, PanelLevels.Default.holder.transform)
                .GetComponent<PanelLevelsCell>();
            cell.gameObject.SetActive(false);
            if (fake)
            {
                fakeCells.Add(cell);
            }
            else
            {
                cell.linkedLevel = level;
                cells.Add(cell);
            }
            if (level is LevelLegendary || fakeLegendary)
            {
                cell.imageBG.sprite = PanelLevels.Default.spriteLegendaryBG;
                cell.imageUnknown.sprite = PanelLevels.Default.spriteLegendLevelUnknown;
            }

            return cell;
        }
    }
}
