CREATE DATABASE IF NOT EXISTS personal_cabinet;
USE personal_cabinet;

-- Таблица пользователей
CREATE TABLE IF NOT EXISTS users (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    full_name LONGTEXT NOT NULL,
    link_role LONGTEXT NOT NULL,
    post LONGTEXT NOT NULL
) ENGINE=InnoDB;

-- Таблица образовательных программ
CREATE TABLE IF NOT EXISTS educational_programs (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    code_referral LONGTEXT NOT NULL,
    Name LONGTEXT NOT NULL,
    educational_level LONGTEXT NOT NULL,
    year_approvals DATETIME(6) NULL,
    Status LONGTEXT NOT NULL,
    user_id INT NOT NULL,
    CONSTRAINT FK_educational_programs_users_user_id FOREIGN KEY (user_id)
        REFERENCES users (Id) ON DELETE CASCADE
) ENGINE=InnoDB;

-- Таблица элементов образовательной программы
CREATE TABLE IF NOT EXISTS educational_program_elements (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    educational_program_id INT NOT NULL,
    type_element LONGTEXT NOT NULL,
    Name LONGTEXT NOT NULL,
    upload_date DATE NULL,
    Description LONGTEXT NOT NULL,
    status_approvals LONGTEXT NOT NULL,
    file_path LONGTEXT NULL,
    file_name LONGTEXT NULL,
    CONSTRAINT FK_educational_program_elements_educational_programs FOREIGN KEY (educational_program_id)
        REFERENCES educational_programs (Id) ON DELETE CASCADE
) ENGINE=InnoDB;

-- Таблица комментариев к элементам
CREATE TABLE IF NOT EXISTS comments_educational_program_element (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    educational_program_element_id INT NOT NULL,
    user_id INT NOT NULL,
    date_time_comment DATETIME(6) NOT NULL,
    comment_content LONGTEXT NOT NULL,
    Status LONGTEXT NOT NULL,
    CONSTRAINT FK_comments_element FOREIGN KEY (educational_program_element_id)
        REFERENCES educational_program_elements (Id) ON DELETE CASCADE,
    CONSTRAINT FK_comments_user FOREIGN KEY (user_id)
        REFERENCES users (Id) ON DELETE CASCADE
) ENGINE=InnoDB;

-- Дополнительные таблицы

CREATE TABLE IF NOT EXISTS departments (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    CodeDepartment LONGTEXT NULL,
    Name LONGTEXT NULL
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS facultys (
    Id CHAR(36) PRIMARY KEY,
    Name LONGTEXT NULL
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS educational_program_managers (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    EducationalProgramLink INT NOT NULL,
    LinkManager INT NOT NULL
) ENGINE=InnoDB;
