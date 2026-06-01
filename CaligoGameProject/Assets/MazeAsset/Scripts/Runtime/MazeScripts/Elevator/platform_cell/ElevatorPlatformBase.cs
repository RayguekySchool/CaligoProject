using System.Collections;
using TMPro;
using UnityEngine;

namespace MazeAsset.MazeGenerator
{
    internal abstract class ElevatorPlatformBase : IElevatorPlatform
    {
        protected bool animating = false;
        protected Mesh platformMesh;
        [SerializeField] protected AnimatorElevatorService animatorService;
        [SerializeField] protected MazeGeneratorManager coroutineOwner;
        protected IEnumerator coroutineButton;
        protected IEnumerator coroutineAnimation;
        [SerializeField] protected PositionCalculatorService positionCalculator;
        [SerializeField] protected DirectionMoveEnum directionMove;
        [SerializeField] protected InteractionElevatorTextController interactionTextController;
        [SerializeField] protected TMP_Text textMeshPro;

        protected ElevatorPlatformBase(MazeGeneratorManager coroutineOwner, TMP_Text textMeshPro, AnimatorElevatorService animatorService)
        {
            this.coroutineOwner = coroutineOwner;
            this.textMeshPro = textMeshPro;
            this.animatorService = animatorService;
            this.positionCalculator = new PositionCalculatorService();
            this.interactionTextController = new InteractionElevatorTextController();
            InitData(1);
        }

        protected abstract void InitData(float width);

        protected virtual IEnumerator CheckButtonPress(GameObject elevatorPlatform)
        {
            while (true)
            {
                if (animating)
                {
                    textMeshPro.text = "";
                    if (coroutineButton != null)
                    {
                        coroutineOwner.StopCoroutine(coroutineButton);
                    }
                }

                var moveDirection = GetMoveDirection();
                if (moveDirection != DirectionMoveEnum.None)
                {
                    MoveElevator(elevatorPlatform, moveDirection);
                }

                yield return null;
            }
        }


        private DirectionMoveEnum GetMoveDirection()
        {
            if (Input.GetKeyDown(KeyCode.Q) && (directionMove == DirectionMoveEnum.Up || directionMove == DirectionMoveEnum.Both))
            {
                return DirectionMoveEnum.Up;
            }
            else if (Input.GetKeyDown(KeyCode.R) && (directionMove == DirectionMoveEnum.Down || directionMove == DirectionMoveEnum.Both))
            {
                return DirectionMoveEnum.Down;
            }
            return DirectionMoveEnum.None;
        }

        private void MoveElevator(GameObject elevatorPlatform, DirectionMoveEnum direction)
        {
            var position = positionCalculator.CalculatePosition(elevatorPlatform);
            GameObject target = null;

            if (direction == DirectionMoveEnum.Up)
            {
                target = GetTargetElevator(position, 1);  // Move up
            }
            else if (direction == DirectionMoveEnum.Down)
            {
                target = GetTargetElevator(position, -1);  // Move down
            }

            if (target != null)
            {
                coroutineOwner.HandleCellEnter(target, true);
                StartElevatorMovement(elevatorPlatform, target, direction);
            }
        }

        private GameObject GetTargetElevator(Vector3 position, int yOffset)
        {
            return GetGOFromGOManager((int)position.x, (int)position.y + yOffset, (int)position.z);
        }

        private void StartElevatorMovement(GameObject elevatorPlatform, GameObject target, DirectionMoveEnum direction)
        {
            coroutineAnimation = direction == DirectionMoveEnum.Up
                ? animatorService.MoveUp(target)
                : animatorService.MoveDown(target);

            StartAnim(elevatorPlatform);
        }

        protected void StartAnim(GameObject elevatorPlatform)
        {
            elevatorPlatform.GetComponent<BoxCollider>().enabled = false;
            animating = true;

            animatorService.platform = elevatorPlatform;
            coroutineOwner.StartCoroutine(StartCoroutineWithCallback(coroutineAnimation, () =>
            {
                animating = false;
            }));

            elevatorPlatform.GetComponent<BoxCollider>().enabled = true;
        }

        private IEnumerator StartCoroutineWithCallback(IEnumerator coroutine, System.Action onComplete)
        {
            yield return coroutineOwner.StartCoroutine(coroutine);
            onComplete?.Invoke();
        }

        protected GameObject GetGOFromGOManager(int x, int y, int z)
        {
            return coroutineOwner.GetElevator(x, y, z);
        }

        internal void Interact(GameObject elevatorPlatform)
        {
            if (animating) return;

            coroutineButton = CheckButtonPress(elevatorPlatform);
            coroutineOwner.StartCoroutine(coroutineButton);
            UpdateElevatorDirection(elevatorPlatform);

            textMeshPro.text = interactionTextController.GetInteractionText(directionMove);
        }

        private void UpdateElevatorDirection(GameObject elevatorPlatform)
        {
            var position = positionCalculator.CalculatePosition(elevatorPlatform);
            var upper = GetGOFromGOManager((int)position.x, (int)position.y + 1, (int)position.z);
            directionMove = upper != null ? DirectionMoveEnum.Up : DirectionMoveEnum.None;

            var down = GetGOFromGOManager((int)position.x, (int)position.y - 1, (int)position.z);
            if (down != null)
            {
                directionMove = directionMove == DirectionMoveEnum.Up ? DirectionMoveEnum.Both : DirectionMoveEnum.Down;
            }
        }

        internal void StopInteract(GameObject elevatorPlatform)
        {
            coroutineOwner.StopCoroutine(coroutineButton);
            textMeshPro.text = "";
        }

        void IElevatorPlatform.DeleteElevator()
        {
            if (animatorService.doors != null)
            {
                GameObject.DestroyImmediate(animatorService.doors);
            }
        }

        void IElevatorPlatform.ReinitData()
        {
            InitData(1);
        }

        bool IElevatorPlatform.RemoveGOToGOManager(int x, int y, int z)
        {
            return coroutineOwner.RemoveElevator(x, y, z);
        }

        GameObject IElevatorPlatform.GetGOFromManager(int x, int y, int z)
        {
            return GetGOFromGOManager(x, y, z);
        }

        void IElevatorPlatform.AddGOToGOManager(GameObject elevator, int x, int y, int z)
        {
            coroutineOwner.AddElevator(elevator, x, y, z);
        }

        public abstract GameObject GetPlatform(float s);
    }
}
