# Admin Dashboard API Documentation

## Overview

This dashboard provides comprehensive statistics and insights for the LMS (Learning Management System) administrators. It includes system overview metrics and advanced analytics for better decision-making.

## Architecture

- **Location**: `Lssctc.ProgramManagement/Dashboard`
- **Pattern**: Repository Pattern with Unit of Work
- **Framework**: ASP.NET Core 8.0 Web API

## API Endpoints

### Base Route: `api/admin/dashboard`

---

## Part 1: System Overview

### GET `/summary`

**Description**: Get a comprehensive system overview with total counts for all major entities.

**Response**: `SystemSummaryDto`

```json
{
  "totalPrograms": 15,
  "totalCourses": 42,
  "totalTrainees": 350,
  "totalInstructors": 28,
  "totalClasses": 85,
  "totalPractices": 120
}
```

**Business Rules**:
- Programs: Only active and not deleted programs
- Courses: Only active and not deleted courses
- Trainees: All trainees that are not deleted
- Instructors: All instructors that are not deleted
- Classes: All classes regardless of status
- Practices: All practices that are not deleted

---

## Part 2: Advanced Statistics

### GET `/courses/popular?top={number}`

**Description**: Get the top N most popular courses based on enrollment count.

**Query Parameters**:
- `top` (optional): Number of results to return (default: 5, max: 20)

**Response**: `IEnumerable<PopularCourseDto>`

```json
[
  {
    "courseName": "Mobile Crane Operations",
    "totalEnrollments": 145
  },
  {
    "courseName": "Tower Crane Safety",
    "totalEnrollments": 132
  }
]
```

**Logic**:
- Path: `Enrollment -> Class -> ProgramCourse -> Course`
- Groups all enrollments by course
- Counts total enrollments per course
- Orders by enrollment count descending

---

### GET `/trainees/active?top={number}`

**Description**: Get the top N most active trainees based on the number of distinct courses they are enrolled in.

**Query Parameters**:
- `top` (optional): Number of results to return (default: 5, max: 20)

**Response**: `IEnumerable<ActiveTraineeDto>`

```json
[
  {
    "traineeName": "John Smith",
    "traineeCode": "TRN-2024-001",
    "enrolledCourseCount": 8
  },
  {
    "traineeName": "Sarah Johnson",
    "traineeCode": "TRN-2024-002",
    "enrolledCourseCount": 7
  }
]
```

**Logic**:
- Counts distinct courses per trainee
- Uses trainee's full name (fallback to username if not available)
- Only includes non-deleted trainees
- Orders by enrolled course count descending

---

### GET `/classes/status-distribution`

**Description**: Get the distribution of classes across all status categories.

**Response**: `IEnumerable<ClassStatusDistributionDto>`

```json
[
  {
    "statusName": "Draft",
    "count": 12
  },
  {
    "statusName": "Open",
    "count": 15
  },
  {
    "statusName": "Inprogress",
    "count": 35
  },
  {
    "statusName": "Completed",
    "count": 20
  },
  {
    "statusName": "Cancelled",
    "count": 3
  }
]
```

**Class Status Enum**:
- `Draft` (1): Class is being prepared
- `Open` (2): Class is open for enrollment
- `Inprogress` (3): Class has started
- `Completed` (4): Class has finished
- `Cancelled` (5): Class was cancelled

**Logic**:
- Groups all classes by their status
- Ensures all 5 status types are represented (with 0 count if none exist)
- Useful for pie charts or bar graphs

---

### GET `/completions/trends?year={year}`

**Description**: Get monthly course completion trends for a specific year.

**Query Parameters**:
- `year` (optional): Year to get trends for (default: current year)

**Response**: `IEnumerable<CourseCompletionTrendDto>`

```json
[
  {
    "month": 1,
    "monthName": "January",
    "completedCount": 15
  },
  {
    "month": 2,
    "monthName": "February",
    "completedCount": 22
  },
  ...
  {
    "month": 12,
    "monthName": "December",
    "completedCount": 18
  }
]
```

**Logic**:
- Queries `TraineeCertificate` table for issued certificates
- Filters by the specified year
- Groups by month (1-12)
- Returns data for all 12 months (fills 0 if no completions)
- Uses current culture for month names

**Use Cases**:
- Line charts showing completion trends over time
- Seasonal analysis of course completions
- Capacity planning for upcoming months

---

## DTOs (Data Transfer Objects)

### SystemSummaryDto
```csharp
public class SystemSummaryDto
{
    public int TotalPrograms { get; set; }
    public int TotalCourses { get; set; }
    public int TotalTrainees { get; set; }
    public int TotalInstructors { get; set; }
    public int TotalClasses { get; set; }
    public int TotalPractices { get; set; }
}
```

### PopularCourseDto
```csharp
public class PopularCourseDto
{
    public string CourseName { get; set; }
    public int TotalEnrollments { get; set; }
}
```

### ActiveTraineeDto
```csharp
public class ActiveTraineeDto
{
    public string TraineeName { get; set; }
    public string TraineeCode { get; set; }
    public int EnrolledCourseCount { get; set; }
}
```

### ClassStatusDistributionDto
```csharp
public class ClassStatusDistributionDto
{
    public string StatusName { get; set; }
    public int Count { get; set; }
}
```

### CourseCompletionTrendDto
```csharp
public class CourseCompletionTrendDto
{
    public int Month { get; set; }
    public string MonthName { get; set; }
    public int CompletedCount { get; set; }
}
```

---

## Service Interface

```csharp
public interface IAdminDashboardService
{
    Task<SystemSummaryDto> GetSystemSummaryAsync();
    Task<IEnumerable<PopularCourseDto>> GetTopPopularCoursesAsync(int topCount = 5);
    Task<IEnumerable<ActiveTraineeDto>> GetTopActiveTraineesAsync(int topCount = 5);
    Task<IEnumerable<ClassStatusDistributionDto>> GetClassStatusDistributionAsync();
    Task<IEnumerable<CourseCompletionTrendDto>> GetCourseCompletionTrendsAsync(int year);
}
```

---

## Technical Implementation Details

### Performance Optimizations

1. **AsNoTracking()**: All queries use `AsNoTracking()` since we're only reading data for reports
2. **Materialization Strategy**: Complex grouping operations are materialized before projection to ensure SQL compatibility
3. **Efficient Joins**: Uses `Include()` and `ThenInclude()` to load related entities in a single query

### Entity Relationships Used

```
Enrollment ? Class ? ProgramCourse ? Course
Enrollment ? Trainee ? User
Class ? ClassInstructor ? Instructor
TraineeCertificate ? Enrollment ? Trainee
```

### Database Queries

All queries follow these principles:
- Use `GetAllAsQueryable()` from the repository pattern
- Apply `AsNoTracking()` for read-only operations
- Use proper `Include()` chains for navigation properties
- Filter deleted records appropriately

---

## Usage Examples

### Frontend Integration (JavaScript)

```javascript
// Get system summary
const summary = await fetch('/api/admin/dashboard/summary');
const data = await summary.json();

// Get top 10 popular courses
const courses = await fetch('/api/admin/dashboard/courses/popular?top=10');
const popularCourses = await courses.json();

// Get completion trends for 2024
const trends = await fetch('/api/admin/dashboard/completions/trends?year=2024');
const monthlyData = await trends.json();
```

### Chart.js Integration Example

```javascript
// Line chart for completion trends
const ctx = document.getElementById('completionChart');
new Chart(ctx, {
    type: 'line',
    data: {
        labels: trends.map(t => t.monthName),
        datasets: [{
            label: 'Course Completions',
            data: trends.map(t => t.completedCount),
            borderColor: 'rgb(75, 192, 192)',
            tension: 0.1
        }]
    }
});

// Pie chart for class status distribution
const pieCtx = document.getElementById('statusChart');
new Chart(pieCtx, {
    type: 'pie',
    data: {
        labels: distribution.map(d => d.statusName),
        datasets: [{
            data: distribution.map(d => d.count),
            backgroundColor: [
                'rgb(255, 205, 86)',
                'rgb(54, 162, 235)',
                'rgb(255, 159, 64)',
                'rgb(75, 192, 192)',
                'rgb(255, 99, 132)'
            ]
        }]
    }
});
```

---

## Deployment Notes

### Service Registration

The service is registered in `Program.cs`:

```csharp
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
```

### Dependencies

- `IUnitOfWork`: Provides access to all repositories
- `Microsoft.EntityFrameworkCore`: For LINQ queries and AsNoTracking
- `System.Globalization`: For culture-aware month names

---

## Future Enhancements

Potential improvements to consider:

1. **Caching**: Add response caching for frequently accessed data
2. **Date Range Filters**: Allow custom date ranges for trends
3. **Export Functionality**: Add CSV/Excel export capabilities
4. **Real-time Updates**: Implement SignalR for live dashboard updates
5. **Filtering Options**: Add filters by program, instructor, etc.
6. **More Metrics**: 
   - Average class capacity utilization
   - Instructor workload distribution
   - Revenue analytics (if applicable)
   - Student performance metrics
   - Enrollment vs completion rates

---

## Error Handling

All endpoints return:
- **200 OK**: Successful request with data
- **400 Bad Request**: Invalid parameters (e.g., invalid year)
- **500 Internal Server Error**: Unexpected errors (logged for debugging)

---

## Testing Recommendations

### Unit Tests
- Test each service method independently
- Mock IUnitOfWork and repositories
- Verify correct filtering logic
- Test edge cases (empty data, null values)

### Integration Tests
- Test full API endpoints
- Verify correct SQL query generation
- Test with real database data
- Performance testing with large datasets

---

## Contact & Support

For questions or issues related to the Admin Dashboard:
- Check the codebase documentation
- Review the entity relationship diagrams
- Consult with the development team

---

**Version**: 1.0  
**Last Updated**: 2024  
**Author**: GitHub Copilot  
**Project**: Lssctc LMS Platform
