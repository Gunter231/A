USE personal_cabinet;

-- Добавление тестового пользователя
INSERT INTO users (Id, full_name, link_role, post)
VALUES (1, 'Иванов Иван Иванович', 'Manager', 'Заведующий кафедрой')
ON DUPLICATE KEY UPDATE full_name=VALUES(full_name);

-- Добавление образовательной программы
INSERT INTO educational_programs (Id, code_referral, Name, educational_level, Status, user_id)
VALUES (1, '01.03.02', 'Прикладная математика и информатика', 'Бакалавриат', 'Активна', 1)
ON DUPLICATE KEY UPDATE Name=VALUES(Name);

-- Добавление элементов с разными статусами
INSERT INTO educational_program_elements (Id, educational_program_id, type_element, Name, Description, status_approvals)
VALUES
(1, 1, 'Main', 'Учебный план (очный)', 'Основной учебный план', ''),
(2, 1, 'Main', 'Пояснительная записка', 'Общая информация', 'Отклонено'),
(3, 1, 'Main', 'Календарный учебный график', 'График на 2024 год', 'На рассмотрении'),
(4, 1, 'Discipline', 'Философия', 'Б1.О.01', 'Принято'),
(5, 1, 'Discipline', 'Высшая математика', 'Б1.О.02', '')
ON DUPLICATE KEY UPDATE status_approvals=VALUES(status_approvals);
