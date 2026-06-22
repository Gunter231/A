USE personal_cabinet;

-- Роли
INSERT INTO roles (Id, Name, Description) VALUES
(1, 'Manager', 'Руководитель ОПОП'),
(2, 'Approver', 'Согласующий'),
(3, 'Moderator', 'Модератор'),
(4, 'Admin', 'Администратор')
ON DUPLICATE KEY UPDATE Name=VALUES(Name);

-- Пользователи
INSERT INTO users (Id, full_name, link_role, post) VALUES
(1, 'Иванов Иван Иванович', 'Manager', 'Заведующий кафедрой'),
(2, 'Петрова Анна Сергеевна', 'Approver', 'Декан факультета'),
(3, 'Сидоров Петр Алексеевич', 'Moderator', 'Модератор'),
(4, 'Козлова Мария Ивановна', 'Admin', 'Администратор')
ON DUPLICATE KEY UPDATE full_name=VALUES(full_name);

-- Факультеты
INSERT INTO facultys (Id, Name) VALUES
(1, 'Факультет информационных технологий'),
(2, 'Факультет математики и механики'),
(3, 'Факультет педагогического образования')
ON DUPLICATE KEY UPDATE Name=VALUES(Name);

-- Кафедры
INSERT INTO departments (Id, code_department, Name) VALUES
(1, 'Каф.ПМИ', 'Кафедра прикладной математики и информатики'),
(2, 'Каф.ИВТ', 'Кафедра информационных вычислительных технологий'),
(3, 'Каф.МАТЕМ', 'Кафедра математического анализа')
ON DUPLICATE KEY UPDATE Name=VALUES(Name);

-- Образовательные программы
INSERT INTO educational_programs (Id, code_referral, Name, educational_level, Status, user_id) VALUES
(1, '01.03.02', 'Прикладная математика и информатика', 'Бакалавриат', 'Разрабатывается', 1),
(2, '09.03.01', 'Информатика и вычислительная техника', 'Бакалавриат', 'Разрабатывается', 1),
(3, '44.03.05', 'Педагогическое образование (Математика. Информатика)', 'Бакалавриат', 'Разрабатывается', 1)
ON DUPLICATE KEY UPDATE Name=VALUES(Name);

-- Руководители ОПОП
INSERT INTO educational_program_managers (Id, educational_program_id, user_id) VALUES
(1, 1, 1),
(2, 2, 1),
(3, 3, 1)
ON DUPLICATE KEY UPDATE educational_program_id=VALUES(educational_program_id);

-- Закрепление ОПОП за кафедрами и факультетами
INSERT INTO educational_program_assignments (Id, educational_program_id, department_id, faculty_id) VALUES
(1, 1, 1, 1),
(2, 2, 2, 1),
(3, 3, 1, 3)
ON DUPLICATE KEY UPDATE department_id=VALUES(department_id);

-- Элементы ОПОП (основные документы)
INSERT INTO educational_program_elements (Id, educational_program_id, type_element, Name, Description, status_approvals) VALUES
(1, 1, 'Main', 'Учебный план (очный)', 'Основной учебный план', ''),
(2, 1, 'Main', 'Пояснительная записка', 'Общая информация', 'На доработку'),
(3, 1, 'Main', 'Календарный учебный график', 'График обучения', ''),
(4, 1, 'Main', 'Программа воспитательной работы', 'Воспитательная программа', ''),
(5, 1, 'Main', 'Календарный план воспитательной работы', 'Календарный план', '')
ON DUPLICATE KEY UPDATE status_approvals=VALUES(status_approvals);

-- Элементы ОПОП (дисциплины)
INSERT INTO educational_program_elements (Id, educational_program_id, type_element, Name, Description, status_approvals) VALUES
(6, 1, 'Discipline', 'Философия', 'Б1.О.01', 'Согласовано'),
(7, 1, 'Discipline', 'Математический анализ', 'Б1.О.02', 'На рассмотрении'),
(8, 1, 'Discipline', 'Линейная алгебра', 'Б1.О.03', ''),
(9, 1, 'Discipline', 'Программирование', 'Б1.О.04', 'Согласовано'),
(10, 1, 'Discipline', 'Базы данных', 'Б1.О.05', '')
ON DUPLICATE KEY UPDATE status_approvals=VALUES(status_approvals);

-- Элементы ОПОП (практики)
INSERT INTO educational_program_elements (Id, educational_program_id, type_element, Name, Description, status_approvals) VALUES
(11, 1, 'Practice', 'Учебная практика', 'Практика 1', ''),
(12, 1, 'Practice', 'Производственная практика', 'Практика 2', '')
ON DUPLICATE KEY UPDATE status_approvals=VALUES(status_approvals);

-- Элементы ОПОП (ГИА)
INSERT INTO educational_program_elements (Id, educational_program_id, type_element, Name, Description, status_approvals) VALUES
(13, 1, 'GIA', 'Государственный экзамен', 'ГИА', ''),
(14, 1, 'GIA', 'Выпускная квалификационная работа', 'ВКР', '')
ON DUPLICATE KEY UPDATE status_approvals=VALUES(status_approvals);
