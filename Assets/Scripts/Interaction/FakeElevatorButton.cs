using UnityEngine;

// Кнопка лифта. Наводишь прицел, жмёшь E.
//   - Кнопка этажа в кабине (Is Call Button выключено): везёт на этаж Floor.
//   - Кнопка вызова снаружи (Is Call Button включено): открывает двери лифта на этом этаже.
// Вешается на кнопку (у неё должен быть Collider).
public class FakeElevatorButton : MonoBehaviour, IInteractable
{
    public FakeElevator elevator;
    [Tooltip("Кнопка в кабине: куда везти (0 = первый этаж, 1 = второй, 2 = третий)")]
    public int floor = 0;
    [Tooltip("Галочка — это кнопка вызова снаружи, а не кнопка этажа в кабине")]
    public bool isCallButton = false;

    public string GetPrompt()
    {
        if (elevator == null) return "Лифт не работает";
        if (!elevator.powered) return "Лифт обесточен";
        if (elevator.IsRiding) return "Лифт едет...";

        int here = elevator.FloorIndexAt(transform.position.y);
        if (isCallButton)
            return elevator.AreDoorsOpen(here) ? "Лифт здесь" : "Вызвать лифт";
        if (here == floor) return "Вы на этаже " + (floor + 1);
        if (floor == elevator.lockedFloor && !elevator.HasFloorKey()) return "Этаж " + (floor + 1) + " — нужен ключ";
        return "Этаж " + (floor + 1);
    }

    public void Interact(PlayerInteraction player)
    {
        if (elevator == null) return;
        if (isCallButton) elevator.Call(elevator.FloorIndexAt(transform.position.y));
        else elevator.Ride(floor);
    }
}
