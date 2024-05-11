FEMmini
======
МКЭ программа для решения плоских задач типа plane stress/plane strain.
Программа состоит из трех основных модулей:
- Решатель. Включает в себя всю часть, относящуюся к механике и математике.
- Графический движок. Занимается рендером графики. Используется библиотека OpenGL, через обертку OpenTK.
- Графический интерфейс. Реализован на wpf с использованием паттерна MVVM.

Screenshots
------
https://ibb.co/fXxpfZh
![screenshot](miniFEM window.png)
![]([https://image.ibb.co/nt76La/Screenshot_from_2017_02_23_13_26_44.png](https://ibb.co/fXxpfZh))

Progress
------
Решатель
- ✅ Линейный решатель
- 🔄 Учет геометрической нелинейности. Нагрузка разбивается на шаги, на каждом шаге пересчитывается матрица жесткости
- ❌ Физически нелинейные модели материалов
- ❌ Мешинг сетки конечных элементов

Графический движок
- ✅ Рендер геометрии. В исходном виде и деформированная схема
- ✅ Отображение условных обозначений: граничные условия, нагрузки, типы жесткостей
- 🔄 Рендер текста
- 🔄 Отображение результатов расчета: перемещения, напряжения, пластические точки
- ❌ Редактирование схемы: добавление узлов, элементов и т.д.

Structure
------
❌ В планах добавить дерево классов, используемых в проекте

Dependencies
------
- Решатель мкэ самописный, из готовых библиотек используется линейная алгебра MathNet
- Графическа библиотека OpenTK
- Интерфейс WPF
