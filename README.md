# IO модуль

## Описание
Модуль IO должен считывать исходные данные из файла и сохранять их в буфер и уже из буфера передает литеры анализатору. 
Также данный модуль отвечает за вывод ошибок. IO модуль должен иметь возможность принимать сообщения об ошибках из других модулей иметь интерфейс вывода.

## Проектирование
Модуль IO построен в виде отдельного класса IOModule.
Класс содержит следующие методы:
- получение текущей литеры
- получение позиции текущей литеры
- получение следующей литеры
- получение подстроки текущей строки
- добавление ошибки
- вывод всех ошибок
Данные методы являются публичными для того, чтобы другие модули в последствии могли обратиться к ним.

Также в классе содержатся следующие поля:
- поток чтения
- текущая строка
- номер текущей строки
- номер позиции в текущей строке
- перечислимый тип для хранения кодов ошибок
- словарь для хранения строк и ошибок в них

## Реализация
Модуль реализован следующим образом:
Модуль считывает исходную программу из файла построчно, одновременно сохраняя только одну строку. При вызове соответствующего метода возвращает литеру текущей строки анализатору. Если литеры текущей строки кончились, то модуль считывает следующую строку.

Модуль также занимается хранением и выводом ошибок, поэтому в нем присутствует отдельная структура данных, которая хранит ошибки. В качестве такой структуры данных я выбрал словарь, так как для одной строки может быть несколько ошибок из нескольких модулей. 
И сделано это так, что ключ словаря - это номер строки, а значение - это сама строка и список кодов ошибок для нее.
Вывод ошибок производится отдельным методом, в котором выводится строка, коды ошибок с пояснительным текстом. В конце выводится общее число найденных ошибок.

Коды ошибок представлены в виде перечисляемого типа CompilerError. Строки с пояснительной информацией ошибки хранятся в словарь, где ключ - это код ошибки, а значение - информация об ошибке.

# Лексический анализатор 
Задача лексического анализатора (сканера) формировать символы исходной программы и строить их внутреннее представление. Также ЛА должен распознавать и исключать комментарии, которые не нужны для дальнейшей трансляции.

## Лексер
### Проектирование
Сканер реализован в виде отдельного класса Lexer.
Lexer содержит следующие методы и свойства: 
- методы для игнорирования ненужных символов - пробел, табуляция, и комментариев
- метод для чтения целых, вещественных беззнаковых констант
- метод для чтения идентификатора и ключевого слова
- метод для получения токена
- метод для получения следующей литеры
- свойство для получения позиции текущей литеры
- свойство для получения текущей литеры

Lexer содержит следующие поля:
- поле модуля IO
- словарь спец. символов

### Реализация
Работа модуля организована следующим образом:
При создании экземпляра лексического модуля, в конструкторе передается IO модуль.
С помощью IO модуля лексер получает литеры, а также может сохранять найденные на данном этапе ошибки.
На основе получаемых литер лексер производит разбор исходного текста программа, выделяет лексемы в соответствии с их описанием и возвращает токены.

## Токены
### Проектирование
Токен представляет собой объект, созданный на основе лексемы во время лексического анализа.
Токены можно представить в виде иерархии классов, разбив их по смыслу - токены для идентификатора, спец. символа, констант и токен для всего остального.
### Реализация
Я сделал это следующим образом - в качестве базового класса используется абстрактный класс, который содержит поле - позиция токена в строке.
Далее токены делятся 4 класса, все из которых наследуются от базового.
- Первый класс IdentifierToken - предназначен для идентификаторов и содержит одно поле строкового типа для хранения имени идентификатора.
- Второй класс SpecialSymbolToken - предназначен для специальных символов и содержит одно поле для хранения идентификатора перечислимого типа спец. символов.
- Третий класс ConstToken - данный класс является обобщенным и предназначен для хранения констант различного типа (int, double, string, bool).
- Последний класс TriviaToken - предназначен для ошибочных токенов и также для токена конца файла, чтобы следующий модуль мог понять, когда чтение программы закончилось.

## Тестирование IO + Лексер
На вход был подан следующий текст программы на языке Паскаль.
```pas

program HelloWorld(output);
var s:string;
    b:real;
begin
    b:=2 * 2 / 100 + 1 - 15;
    {comment section}
    b := 0.1234;
    s:= 's t';
    writeln('Hello, World!')
end.
```

На выходе были получены следующие токены:
```txt
        Token type                     Value
SpecialSymbolToken              ProgramToken
   IdentifierToken                HelloWorld
SpecialSymbolToken     LeftRoundBracketToken
   IdentifierToken                    output
SpecialSymbolToken    RightRoundBracketToken
SpecialSymbolToken            SemicolonToken
SpecialSymbolToken                  VarToken
   IdentifierToken                         s
SpecialSymbolToken                ColonToken
   IdentifierToken                    string
SpecialSymbolToken            SemicolonToken
   IdentifierToken                         b
SpecialSymbolToken                ColonToken
   IdentifierToken                      real
SpecialSymbolToken            SemicolonToken
SpecialSymbolToken                BeginToken
   IdentifierToken                         b
SpecialSymbolToken           AssignmentToken
         Int token                         2
SpecialSymbolToken                 MultToken
         Int token                         2
SpecialSymbolToken             DivisionToken
         Int token                       100
SpecialSymbolToken                 PlusToken
         Int token                         1
SpecialSymbolToken                MinusToken
         Int token                        15
SpecialSymbolToken            SemicolonToken
   IdentifierToken                         b
SpecialSymbolToken           AssignmentToken
      Double token                    0,1234
SpecialSymbolToken            SemicolonToken
   IdentifierToken                         s
SpecialSymbolToken           AssignmentToken
      String token                       s t
SpecialSymbolToken            SemicolonToken
   IdentifierToken                   writeln
SpecialSymbolToken     LeftRoundBracketToken
      String token             Hello, World!
SpecialSymbolToken    RightRoundBracketToken
SpecialSymbolToken                  EndToken
SpecialSymbolToken                  DotToken
 TriviaToken token            EndOfFileToken
```
Результаты соответствуют ожидаемым значениям, поэтому тестирование пройдено.