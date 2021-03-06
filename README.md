<p align="center"> 
  <img align="center" src="https://github.com/0xLaileb/Fan-Bots/blob/master/GITHUB_RESOURCES/logo.png?raw=true" width="150"/> 
</p>

<h1><div align="center">Fan-Bot's</h1>
<p align="center">
  <img src="https://img.shields.io/badge/PRICE-free-%231DC8EE"/>
  <img src="https://img.shields.io/badge/.NET_FRAMEWORK-4.7-%231DC8EE"/>
  <img src="https://img.shields.io/badge/SUPPORT-no-%231DC8EE"/>
</p>

<p align="center">
  <img src="https://img.shields.io/github/downloads/0xLaileb/Fan-Bots/total?color=%231DC8EE&label=DOWNLOADS&logo=GitHub&logoColor=%231DC8EE&style=flat"/>
  <img src="https://img.shields.io/github/last-commit/0xLaileb/Fan-Bots?color=%231DC8EE&label=LAST%20COMMIT&style=flat"/>
  <img src="https://img.shields.io/github/release-date/0xLaileb/Fan-Bots?color=%231DC8EE&label=RELEASE%20DATE&style=flat"/>
</p>

<p align="center">
  Данное <b>бесплатное ПО</b> предназначено для запуска ботов на игровые сервера Warface.<br>
  Группа ВК: <a href="https://vk.com/fanbots_wf" target="_blank">Fan-Bot's ● (Fun-Bot's for Warface)</a><br>
  Связанный проект: <a href="https://github.com/0xLaileb/warfacebot_fb" target="_blank">WarfaceBotFB</a><br>
</p>

[wf_ru]: https://ru.warface.com/
[releases]: https://github.com/0xLaileb/Fan-Bots/releases/
[commands_wb]: https://github.com/0xLaileb/warfacebot_fb#команды

## 🤖 Основной функционал

- Запуск ботов (warfacebot_fb) на игровые сервера Warface.
- Авторизация MailRu, MyCom, GoPlay.
- Поддержка двухфакторной авторизации.
- Настройка запуска, ботов, цветов и т.д.
- Автоматические команды (выполняются при запуске бота).
- Автоматическое обновление ключей.
- Поддержка нескольких языков (требует доработки).
- Запуск всех ботов и их рестарт.
- Сохранение данных аккаунта, настроек и логов (авторизации и ботов).

## 🔎 Основной лаунчер
![](https://github.com/0xLaileb/Fan-Bots/blob/master/GITHUB_RESOURCES/gui.gif?raw=true)

## 🚀 Как использовать

- ### Безопасный запуск
Запускайте на **виртуальной машине**, где **НЕТ** **Warface** и **Игрового Центра**, иначе вы можете получить **блокировку основного аккаунта** (который запущен в игре Warface на данный момент).

- ### Поддержка: @mail.ru | @inbox.ru | @list.ru | @bk.ru | в будущем возможно и другие

- ### Вы должны иметь игровой аккаунт в Warface

1. Вы должны зарегистрироваться на нужном вам сервере (для RU: **[ru.warface.com][wf_ru]**). 
2. Ваш игровой аккаунт для бота должен быть свободен.
3. Он не должен быть заблокирован.

- ### Запуск бота
1. Скачайте последний релиз **Fan-Bot's** : **[Releases][releases]**
2. Перекиньте папку `Fan-Bot's` в удобное место.
3. Запустите `START.exe`.
4. Выберите `сервер`.
5. Введите `логин` и `пароль` от игрового аккаунта, который будет ботом.
6. Нажмите `ВОЙТИ`.

- ### Команды можете узнать [тут][commands_wb].

## 🔧 Для разработчиков
- ### Информация о проекте
1. Целевая рабочая среда: `.Net Framework 4.7`
2. Среда разработки: `Visual Studio 2019`

- ### Сборка проекта
1. Установите Visual Studio (не забудьте поставить C# и .Net Framework 4.7).
2. Скачайте данный исходный код (лучше из релиза).
3. Выполните сборку проекта по очереди (`AUTHORIZATION` -> `LAUNCHER_FANBOT` -> `START`).
4. Запускайте по [инструкции запуска](https://github.com/0xLaileb/Fan-Bots#как-использовать).

- ### Список, что нужно исправить
1. Переписать авторизацию EU.
2. Добавить поддержку @yandex, @gmail и т.д к RU авторизации.
