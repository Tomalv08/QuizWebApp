using Microsoft.Data.Sqlite;
using QuizWebBlazor.Models;
using System;
using System.Collections.Generic;

namespace QuizWebBlazor.Services
{
    public class QuizDB
    {
        private SqliteConnection connection;

        public void OpenConnection(string databaseFileName)
        {
            try
            {
                connection = new SqliteConnection("Data Source=" + databaseFileName);
                connection.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fehler beim Öffnen der Datenbankverbindung: " + ex.Message);
                throw;
            }
        }

        public void CloseConnection()
        {
            if (connection != null)
            {
                connection.Close();
                connection = null;
            }
        }

        public void CreateTables()
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Quiz(
                    [ID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 
                    [Title] NVARCHAR(100) NOT NULL,
                    [Description] NVARCHAR(800)
                )";
                command.ExecuteNonQuery();

                command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Question(
                    [ID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 
                    [QuizID] INTEGER NOT NULL,
                    [Text] NVARCHAR(800) NOT NULL,
                    FOREIGN KEY (QuizID) REFERENCES Quiz(ID)
                )";
                command.ExecuteNonQuery();

                command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Answer(
                    [ID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 
                    [QuestionID] INTEGER NOT NULL,
                    [Text] NVARCHAR(800) NOT NULL,
                    [IsCorrect] BOOLEAN NOT NULL,
                    FOREIGN KEY (QuestionID) REFERENCES Question(ID)
                )";
                command.ExecuteNonQuery();
            }
        }

        public void InsertQuiz(Quiz quiz)
        {
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                        INSERT INTO Quiz (Title, Description) 
                        VALUES ($Title, $Description);
                        SELECT last_insert_rowid();";
                        command.Parameters.AddWithValue("$Title", quiz.Title);
                        command.Parameters.AddWithValue("$Description", quiz.Description);
                        command.ExecuteScalar();
                    }
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Fehler beim Speichern des Quiz: " + ex.Message);
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public List<Quiz> GetQuizzes()
        {
            var list = new List<Quiz>();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM Quiz";

                var result = command.ExecuteReader();
                while (result.Read())
                {
                    Quiz quiz = new Quiz
                    {
                        Id = result.GetInt32(0),
                        Title = result.GetString(1),
                        Description = result.GetString(2)
                    };
                    list.Add(quiz);
                }
            }
            return list;
        }

        public Quiz GetQuizById(int quizId)
        {
            Quiz quiz = null;
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM Quiz WHERE ID = $QuizId";
                command.Parameters.AddWithValue("$QuizId", quizId);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        quiz = new Quiz
                        {
                            Id = reader.GetInt32(0),
                            Title = reader.GetString(1),
                            Description = reader.GetString(2)
                        };
                    }
                }
            }
            return quiz;
        }

        public void DeleteQuiz(int quizId)
        {
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "DELETE FROM Quiz WHERE ID = $QuizId";
                        command.Parameters.AddWithValue("$QuizId", quizId);
                        command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Fehler beim Löschen des Quiz: " + ex.Message);
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public void UpdateQuiz(Quiz quiz)
        {
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                        UPDATE Quiz 
                        SET Title = $Title, Description = $Description 
                        WHERE ID = $ID";
                        command.Parameters.AddWithValue("$Title", quiz.Title);
                        command.Parameters.AddWithValue("$Description", quiz.Description);
                        command.Parameters.AddWithValue("$ID", quiz.Id);
                        command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Fehler beim Aktualisieren des Quiz: " + ex.Message);
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }
}
