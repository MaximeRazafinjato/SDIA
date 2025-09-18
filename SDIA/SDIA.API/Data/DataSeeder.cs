using Microsoft.EntityFrameworkCore;
using SDIA.Core.Registrations;
using SDIA.Core.FormTemplates;
using SDIA.Core.Documents;
using SDIA.Core.Users;
using SDIA.SharedKernel.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDIA.API.Data
{
    public class DataSeeder
    {
        private readonly SDIADbContext _context;
        private readonly Random _random = new Random();

        // Listes de données pour générer des noms aléatoires
        private readonly string[] _firstNames = { "Jean", "Marie", "Pierre", "Sophie", "Lucas", "Emma", "Thomas", "Léa", "Alexandre", "Julie", "Nicolas", "Laura", "Antoine", "Camille", "Paul" };
        private readonly string[] _lastNames = { "Martin", "Bernard", "Dubois", "Thomas", "Robert", "Richard", "Petit", "Durand", "Leroy", "Moreau", "Simon", "Laurent", "Michel", "Garcia", "David" };
        private readonly string[] _domains = { "gmail.com", "outlook.com", "yahoo.fr", "hotmail.com", "orange.fr", "wanadoo.fr" };
        private readonly string[] _cities = { "Paris", "Lyon", "Marseille", "Toulouse", "Nice", "Nantes", "Strasbourg", "Montpellier", "Bordeaux", "Lille" };
        private readonly string[] _programs = { "Licence Informatique", "Master Data Science", "Licence Mathématiques", "Master Intelligence Artificielle", "Licence Physique", "Master Cybersécurité", "DUT Informatique", "Licence Économie" };
        private readonly string[] _schools = { "Université Paris-Saclay", "Sorbonne Université", "Université Lyon 1", "Université de Toulouse", "Université de Nice", "École Polytechnique", "CentraleSupélec", "INSA Lyon" };

        public DataSeeder(SDIADbContext context)
        {
            _context = context;
        }

        public async Task SeedRegistrationsAsync(bool force = false)
        {
            // Vérifier s'il y a déjà des inscriptions non supprimées
            var hasRegistrations = await _context.Registrations.Where(r => !r.IsDeleted).AnyAsync();
            if (hasRegistrations && !force)
            {
                return; // Des données existent déjà
            }

            // Si force est true, supprimer les inscriptions existantes
            if (force)
            {
                var existingRegistrations = await _context.Registrations.ToListAsync();
                _context.Registrations.RemoveRange(existingRegistrations);
                await _context.SaveChangesAsync();
            }

            // Récupérer l'organisation par défaut
            var organization = await _context.Organizations.FirstOrDefaultAsync();
            if (organization == null)
            {
                return; // Pas d'organisation
            }

            // Créer un modèle de formulaire si nécessaire
            var formTemplate = await _context.FormTemplates.FirstOrDefaultAsync();
            if (formTemplate == null)
            {
                formTemplate = new FormTemplate
                {
                    Id = Guid.NewGuid(),
                    Name = "Formulaire d'inscription standard",
                    Description = "Formulaire d'inscription pour les nouveaux étudiants",
                    OrganizationId = organization.Id,
                    IsActive = true,
                    FormSchema = @"[
                        {""name"":""dateNaissance"",""label"":""Date de naissance"",""type"":""date"",""required"":true,""order"":1},
                        {""name"":""adresse"",""label"":""Adresse"",""type"":""text"",""required"":true,""order"":2},
                        {""name"":""ville"",""label"":""Ville"",""type"":""text"",""required"":true,""order"":3},
                        {""name"":""codePostal"",""label"":""Code postal"",""type"":""text"",""required"":true,""order"":4},
                        {""name"":""programmeEtudes"",""label"":""Programme d'études"",""type"":""select"",""required"":true,""order"":5},
                        {""name"":""etablissementOrigine"",""label"":""Établissement d'origine"",""type"":""text"",""required"":false,""order"":6},
                        {""name"":""motivation"",""label"":""Lettre de motivation"",""type"":""textarea"",""required"":true,""order"":7}
                    ]"
                };

                _context.FormTemplates.Add(formTemplate);
                await _context.SaveChangesAsync();
            }

            // Générer 50 inscriptions fictives
            var registrations = new List<Registration>();
            var currentDate = DateTime.Now;

            for (int i = 0; i < 50; i++)
            {
                var firstName = _firstNames[_random.Next(_firstNames.Length)];
                var lastName = _lastNames[_random.Next(_lastNames.Length)];
                var email = $"{firstName.ToLower()}.{lastName.ToLower()}.{_random.Next(1, 999)}@{_domains[_random.Next(_domains.Length)]}";
                var phone = GeneratePhoneNumber();
                var city = _cities[_random.Next(_cities.Length)];
                var program = _programs[_random.Next(_programs.Length)];
                var school = _schools[_random.Next(_schools.Length)];

                // Définir le statut de l'inscription
                RegistrationStatus status = (i % 10) switch
                {
                    0 or 1 => RegistrationStatus.Draft,
                    2 or 3 or 4 => RegistrationStatus.Pending,
                    5 or 6 or 7 => RegistrationStatus.Validated,
                    8 or 9 => RegistrationStatus.Rejected,
                    _ => RegistrationStatus.Pending
                };

                // Dates variables selon le statut
                var createdDate = currentDate.AddDays(-_random.Next(1, 90));
                DateTime? submittedDate = null;
                DateTime? validatedDate = null;
                DateTime? rejectedDate = null;

                if (status != RegistrationStatus.Draft)
                {
                    submittedDate = createdDate.AddDays(_random.Next(1, 5));
                    if (status == RegistrationStatus.Validated)
                    {
                        validatedDate = submittedDate.Value.AddDays(_random.Next(1, 10));
                    }
                    else if (status == RegistrationStatus.Rejected)
                    {
                        rejectedDate = submittedDate.Value.AddDays(_random.Next(1, 10));
                    }
                }

                var birthDate = DateTime.Now.AddYears(-_random.Next(18, 25)).AddDays(-_random.Next(365));

                var registration = new Registration
                {
                    Id = Guid.NewGuid(),
                    RegistrationNumber = $"INS-2024-{(i + 1).ToString().PadLeft(5, '0')}",
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    Phone = phone,
                    BirthDate = birthDate,
                    Status = status,
                    OrganizationId = organization.Id,
                    FormTemplateId = formTemplate.Id,
                    FormData = GenerateFormData(city, program, school, birthDate),
                    EmailVerified = status != RegistrationStatus.Draft,
                    PhoneVerified = status == RegistrationStatus.Validated || (status == RegistrationStatus.Pending && _random.Next(2) == 0),
                    SubmittedAt = submittedDate,
                    ValidatedAt = validatedDate,
                    RejectedAt = rejectedDate,
                    RejectionReason = status == RegistrationStatus.Rejected ? "Dossier incomplet ou prérequis non satisfaits." : "",
                    CreatedAt = createdDate,
                    UpdatedAt = createdDate
                };

                // Ajouter des documents pour certaines inscriptions (initialisé à vide car Documents est une propriété de navigation)
                if (status != RegistrationStatus.Draft && _random.Next(3) != 0)
                {
                    registration.Documents = new List<Document>();
                }

                // Ajouter des commentaires pour les inscriptions traitées
                if (status == RegistrationStatus.Validated || status == RegistrationStatus.Rejected)
                {
                    registration.Comments = GenerateComments(registration.Id, status);
                }

                registrations.Add(registration);
            }

            foreach(var reg in registrations)
            {
                _context.Registrations.Add(reg);
            }

            await _context.SaveChangesAsync();
        }

        private string GeneratePhoneNumber()
        {
            var prefixes = new[] { "06", "07" };
            var prefix = prefixes[_random.Next(prefixes.Length)];
            return $"+33 {prefix} {_random.Next(10, 99)} {_random.Next(10, 99)} {_random.Next(10, 99)} {_random.Next(10, 99)}";
        }

        private string GenerateFormData(string city, string program, string school, DateTime birthDate)
        {
            var postalCode = _random.Next(10000, 99999).ToString();
            var streetNumber = _random.Next(1, 200);
            var streets = new[] { "rue de la République", "avenue Victor Hugo", "boulevard Voltaire", "place de la Nation", "rue du Commerce", "avenue des Champs" };
            var street = streets[_random.Next(streets.Length)];

            var motivations = new[]
            {
                "Je suis passionné par ce domaine depuis mon plus jeune âge et je souhaite approfondir mes connaissances.",
                "Mon objectif est de devenir expert dans ce domaine et contribuer à l'innovation technologique.",
                "Cette formation correspond parfaitement à mon projet professionnel et mes aspirations.",
                "Je souhaite acquérir les compétences nécessaires pour réussir dans ce secteur en pleine évolution.",
                "Ma motivation vient de mon désir d'apprendre et de me spécialiser dans ce domaine passionnant."
            };

            var formData = new
            {
                dateNaissance = birthDate.ToString("yyyy-MM-dd"),
                adresse = $"{streetNumber} {street}",
                ville = city,
                codePostal = postalCode,
                programmeEtudes = program,
                etablissementOrigine = school,
                motivation = motivations[_random.Next(motivations.Length)]
            };

            return System.Text.Json.JsonSerializer.Serialize(formData);
        }

        private List<RegistrationComment> GenerateComments(Guid registrationId, RegistrationStatus status)
        {
            var comments = new List<RegistrationComment>();

            var adminUser = _context.Users.FirstOrDefault(u => u.Role == "Admin");
            if (adminUser == null) return comments;

            var commentTexts = status == RegistrationStatus.Validated
                ? new[]
                {
                    "Dossier validé après vérification complète.",
                    "Candidature acceptée. Bienvenue dans notre établissement.",
                    "Félicitations! Votre inscription a été approuvée."
                }
                : new[]
                {
                    "Dossier rejeté après examen approfondi.",
                    "Candidature non retenue. Les critères d'admission ne sont pas remplis.",
                    "Désolé, votre dossier n'a pas été accepté cette année."
                };

            comments.Add(new RegistrationComment
            {
                Id = Guid.NewGuid(),
                RegistrationId = registrationId,
                AuthorId = adminUser.Id,
                Content = commentTexts[_random.Next(commentTexts.Length)],
                IsInternal = false,
                CreatedAt = DateTime.Now.AddDays(-_random.Next(1, 10))
            });

            return comments;
        }
    }
}