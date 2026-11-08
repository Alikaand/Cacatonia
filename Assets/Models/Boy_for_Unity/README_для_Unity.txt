МАЛЬЧИК ИЗ 313 — как поставить в Unity

1. Перетащи всю эту папку в Assets.
2. Boy.fbx -> вкладка Rig: Animation Type = Humanoid, Avatar Definition = Create From This Model -> Apply.
   Вкладка Materials -> Extract Materials (в эту же папку) -> Apply.
3. Каждый файл в Animations: Rig = Humanoid, Create From This Model -> Apply.
   Вкладка Animation: Loop Time, Loop Pose, все три Bake Into Pose -> Apply.
4. Материалы:
   - кожа (Human.body): Base Map = Boy_Skin_Plain.png
   - глаза (Human.low-poly): Base Map = Boy_Eye_Brown.png
   - ВОЛОСЫ (Hair_Lock_Mat): Base Map = Hair_Lock.png, включить Alpha Clipping (Threshold 0.5), Render Face = Both
   - Hair_Cap_Mat: просто почти чёрный цвет
5. Для пиксельного стиля: у всех картинок Filter Mode = Point, Max Size = 512.
6. Animator Controller -> перетащить клипы, по умолчанию Breathing Idle или Injured Walking.

~6000 треугольников, ходьба: Injured Walking.
