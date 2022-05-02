using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static TeamAlpha.Source.MonoModel;

namespace TeamAlpha.Source
{
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Default { get; private set; }
        public PartType CurType { get; set; }

        public List<AudioSimpleData> audioOnPaint;

        public PlayerController() => Default = this;

        private ModelPart partToIgnore;
        private void Update()
        {
            if (!LayerDefault.Default.Playing)
                return;
            if (Input.GetMouseButton(0))
            {
                Ray ray = CameraManager.Default.cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit = default;
                if (Physics.Raycast(ray, out hit))
                {
                    ModelPart part = hit.collider.GetComponent<ModelPart>();
                    if (part != null && part.owner == LevelController.Current.modelPlayable)
                    {
                        part = FindNearestGoodPart(part);
                        if (part == partToIgnore)
                            return;
                        if (part != null)
                        {
                            if (DataGameMain.Default.VibrationsIsEnabled)
                                Vibration.Vibrate(35);
                            audioOnPaint[UnityEngine.Random.Range(0, audioOnPaint.Count)].Play(ProcessorSoundPool.PoolLevel.GameLevel);
                            part.Paint(CurType, 5f);
                            //ComboCounter.Default.HandleBlockPainted();
                            //LevelController.Current.PaintLeft -= 1;
                            //if (LevelController.Current.PaintLeft == 0)
                            //{
                            //    LevelController.Current.CompleteLevel();
                            //    return;
                            //}
                        }
                    }
                }
            }
            else if (Input.GetMouseButtonUp(0))
                partToIgnore = null;
        }
        private ModelPart FindNearestGoodPart(ModelPart part)
        {
            List<ModelPart> _parts = new List<ModelPart>(LevelController.Current.modelPlayable.PartsPlayable.ToArray());
            _parts.Sort((p1, p2) => Vector3.Distance(p1.transform.position, part.transform.position).
                          CompareTo(Vector3.Distance(p2.transform.position, part.transform.position)));
            ModelPart closestPart = null;
            closestPart = _parts.Find(p =>
                            Vector3.Distance(p.transform.position, part.transform.position) < 1.9f *
                              LevelController.Current.modelPlayable.scaleInGame &&
                            !p.TypePainted.guid.id.Equals(CurType.guid.id) &&
                            p.TypeOrigin.guid.id.Equals(CurType.guid.id));

            if (closestPart != null)
            {
                if (!part.TypeOrigin.guid.id.Equals(CurType.guid.id))
                    partToIgnore = part;
                return closestPart;
            }
            else
                return !part.TypeOrigin.IsBorder && !part.TypePainted.guid.id.Equals(CurType.guid.id) ? part : null;
        }
    }
}
