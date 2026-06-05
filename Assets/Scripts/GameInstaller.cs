using GamePrototype;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class GameInstaller : MonoInstaller
{
    private Controls _controls;
    [SerializeField]
    private CellManager _cellManager;
    [SerializeField]
    private SceneController _controller;

    [SerializeField, Space(15f)]
    private CellPaletteSettings _cellPaletteSettings;

    public override void InstallBindings()
    {
        _controls = new Controls();

        _controls.Game.Enable();
        Container.BindInstance(_controls.Game).AsSingle();
        Container.BindInstance(_cellManager).AsSingle();
        Container.BindInstance(_controller).AsSingle();
        Container.BindInstance(_cellPaletteSettings).AsSingle();

        _cellManager.OnCellClicked += CellManagerOnOnCellClicked;

    }
    private void CellManagerOnOnCellClicked(Cell obj)
    {
        obj.SetSelect(_cellPaletteSettings.SelectCell);
    }
}
