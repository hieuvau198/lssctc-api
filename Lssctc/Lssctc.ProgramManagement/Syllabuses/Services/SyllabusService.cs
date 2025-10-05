using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Lssctc.Share.Entities;
using Lssctc.ProgramManagement.Syllabuses.Dtos;
using Lssctc.Share.Contexts;

namespace Lssctc.ProgramManagement.Syllabuses.Services
{
    public class SyllabusService : ISyllabusService
    {
        private readonly LssctcDbContext _context;

        public SyllabusService(LssctcDbContext context)
        {
            _context = context;
        }


        public async Task<SyllabusDto?> GetSyllabusByIdAsync(int id)
        {
            var syllabus = await _context.Set<Syllabuse>()
                .Include(s => s.SyllabusSections.Where(ss => ss.IsDeleted != true))
                .FirstOrDefaultAsync(s => s.Id == id && s.IsDeleted != true);

            if (syllabus == null) return null;

            return new SyllabusDto
            {
                Id = syllabus.Id,
                Name = syllabus.Name,
                CourseName = syllabus.CourseName,
                CourseCode = syllabus.CourseCode,
                Description = syllabus.Description,
                IsActive = syllabus.IsActive,
                SyllabusSections = syllabus.SyllabusSections
                    .OrderBy(ss => ss.SectionOrder)
                    .Select(ss => new SyllabusSectionDto
                    {
                        Id = ss.Id,
                        SyllabusId = ss.SyllabusId,
                        SectionTitle = ss.SectionTitle,
                        SectionDescription = ss.SectionDescription,
                        SectionOrder = ss.SectionOrder,
                        EstimatedDurationMinutes = ss.EstimatedDurationMinutes
                    }).ToList()
            };
        }

        public async Task<List<SyllabusDto>> GetAllSyllabusesAsync()
        {
            var syllabuses = await _context.Set<Syllabuse>()
                .Include(s => s.SyllabusSections.Where(ss => ss.IsDeleted != true))
                .Where(s => s.IsDeleted != true)
                .ToListAsync();

            return syllabuses.Select(syllabus => new SyllabusDto
            {
                Id = syllabus.Id,
                Name = syllabus.Name,
                CourseName = syllabus.CourseName,
                CourseCode = syllabus.CourseCode,
                Description = syllabus.Description,
                IsActive = syllabus.IsActive,
                SyllabusSections = syllabus.SyllabusSections
                    .OrderBy(ss => ss.SectionOrder)
                    .Select(ss => new SyllabusSectionDto
                    {
                        Id = ss.Id,
                        SyllabusId = ss.SyllabusId,
                        SectionTitle = ss.SectionTitle,
                        SectionDescription = ss.SectionDescription,
                        SectionOrder = ss.SectionOrder,
                        EstimatedDurationMinutes = ss.EstimatedDurationMinutes
                    }).ToList()
            }).ToList();
        }

        public async Task<SyllabusDto> CreateSyllabusAsync(CreateSyllabusDto dto)
        {
            var syllabus = new Syllabuse
            {
                Name = dto.Name,
                CourseName = dto.CourseName,
                CourseCode = dto.CourseCode,
                Description = dto.Description,
                IsActive = dto.IsActive ?? true,
                IsDeleted = false
            };

            _context.Set<Syllabuse>().Add(syllabus);
            await _context.SaveChangesAsync();

            return new SyllabusDto
            {
                Id = syllabus.Id,
                Name = syllabus.Name,
                CourseName = syllabus.CourseName,
                CourseCode = syllabus.CourseCode,
                Description = syllabus.Description,
                IsActive = syllabus.IsActive,
                SyllabusSections = new List<SyllabusSectionDto>()
            };
        }

        public async Task<SyllabusDto?> UpdateSyllabusAsync(int id, UpdateSyllabusDto dto)
        {
            var syllabus = await _context.Set<Syllabuse>()
                .FirstOrDefaultAsync(s => s.Id == id && s.IsDeleted != true);

            if (syllabus == null) return null;

            if(!string.IsNullOrWhiteSpace(dto.Name))
                syllabus.Name = dto.Name;
            
            if(!string.IsNullOrWhiteSpace(dto.CourseName))
                syllabus.CourseName = dto.CourseName;
            
            if(!string.IsNullOrWhiteSpace(dto.CourseCode))
                syllabus.CourseCode = dto.CourseCode;
            
            if(!string.IsNullOrWhiteSpace(dto.Description))
                syllabus.Description = dto.Description;
            
            if(dto.IsActive.HasValue)
                syllabus.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            return new SyllabusDto
            {
                Id = syllabus.Id,
                Name = syllabus.Name,
                CourseName = syllabus.CourseName,
                CourseCode = syllabus.CourseCode,
                Description = syllabus.Description,
                IsActive = syllabus.IsActive,
                SyllabusSections = new List<SyllabusSectionDto>()
            };
        }

        public async Task<bool> DeleteSyllabusAsync(int id)
        {
            var syllabus = await _context.Set<Syllabuse>()
                .FirstOrDefaultAsync(s => s.Id == id && s.IsDeleted != true);

            if (syllabus == null) return false;

            syllabus.IsDeleted = true;
            await _context.SaveChangesAsync();

            return true;
        }

        // ==================== Syllabus Section CRUD ====================

        public async Task<SyllabusSectionDto?> GetSyllabusSectionByIdAsync(int id)
        {
            var section = await _context.Set<SyllabusSection>()
                .FirstOrDefaultAsync(ss => ss.Id == id && ss.IsDeleted != true);

            if (section == null) return null;

            return new SyllabusSectionDto
            {
                Id = section.Id,
                SyllabusId = section.SyllabusId,
                SectionTitle = section.SectionTitle,
                SectionDescription = section.SectionDescription,
                SectionOrder = section.SectionOrder,
                EstimatedDurationMinutes = section.EstimatedDurationMinutes
            };
        }

        public async Task<List<SyllabusSectionDto>> GetSyllabusSectionsBySyllabusIdAsync(int syllabusId)
        {
            var sections = await _context.Set<SyllabusSection>()
                .Where(ss => ss.SyllabusId == syllabusId && ss.IsDeleted != true)
                .OrderBy(ss => ss.SectionOrder)
                .ToListAsync();

            return sections.Select(section => new SyllabusSectionDto
            {
                Id = section.Id,
                SyllabusId = section.SyllabusId,
                SectionTitle = section.SectionTitle,
                SectionDescription = section.SectionDescription,
                SectionOrder = section.SectionOrder,
                EstimatedDurationMinutes = section.EstimatedDurationMinutes
            }).ToList();
        }

        public async Task<SyllabusSectionDto> CreateSyllabusSectionAsync(CreateSyllabusSectionDto dto)
        {
            var section = new SyllabusSection
            {
                SyllabusId = dto.SyllabusId,
                SectionTitle = dto.SectionTitle,
                SectionDescription = dto.SectionDescription,
                SectionOrder = dto.SectionOrder,
                EstimatedDurationMinutes = dto.EstimatedDurationMinutes,
                IsDeleted = false
            };

            _context.Set<SyllabusSection>().Add(section);
            await _context.SaveChangesAsync();

            return new SyllabusSectionDto
            {
                Id = section.Id,
                SyllabusId = section.SyllabusId,
                SectionTitle = section.SectionTitle,
                SectionDescription = section.SectionDescription,
                SectionOrder = section.SectionOrder,
                EstimatedDurationMinutes = section.EstimatedDurationMinutes
            };
        }

        public async Task<SyllabusSectionDto?> UpdateSyllabusSectionAsync(int id, UpdateSyllabusSectionDto dto)
        {
            var section = await _context.Set<SyllabusSection>()
                .FirstOrDefaultAsync(ss => ss.Id == id && ss.IsDeleted != true);

            if (section == null) return null;

            int originalOrder = section.SectionOrder;

            if (dto.SectionOrder > 0 && dto.SectionOrder != originalOrder)
            {
                int newOrder = dto.SectionOrder;
                var swappedSection = await _context.Set<SyllabusSection>()
                    .FirstOrDefaultAsync(ss =>
                        ss.SyllabusId == section.SyllabusId &&
                        ss.SectionOrder == newOrder &&
                        ss.Id != section.Id
                    );
                if (swappedSection != null)
                {
                    swappedSection.SectionOrder = originalOrder;
                }
                section.SectionOrder = newOrder;
            }

            if (dto.SectionTitle != null)
                section.SectionTitle = dto.SectionTitle;
            if (dto.SectionDescription != null)
                section.SectionDescription = dto.SectionDescription;
            if (dto.EstimatedDurationMinutes.HasValue)
                section.EstimatedDurationMinutes = dto.EstimatedDurationMinutes.Value;

            await _context.SaveChangesAsync();

            return new SyllabusSectionDto
            {
                Id = section.Id,
                SyllabusId = section.SyllabusId,
                SectionTitle = section.SectionTitle,
                SectionDescription = section.SectionDescription,
                SectionOrder = section.SectionOrder,
                EstimatedDurationMinutes = section.EstimatedDurationMinutes
            };
        }

        public async Task<bool> DeleteSyllabusSectionAsync(int id)
        {
            var section = await _context.Set<SyllabusSection>()
                .FirstOrDefaultAsync(ss => ss.Id == id && ss.IsDeleted != true);

            if (section == null) return false;

            section.IsDeleted = true;
            await _context.SaveChangesAsync();

            return true;
        }
    }
}