<p align="center">
<img height="300" width="300" src="https://raw.githubusercontent.com/eliesercapillar/MapleTinder/refs/heads/main/frontend/public/logos/mt_mushroom.png" alt="MapleHotties Logo" align="center">
</p><br>

# MapleHotties 🍁

An end-to-end, full-stack web application where [MapleStory](https://www.nexon.com/maplestory/) players can vote on the hottest characters of the week, creating an engaging and fun community experience for fans of silly _mushroom game_.

[maplehotties.com](YOUR_DEPLOYED_URL_HERE) | [API Documentation](./docs/API.md) | [Architecture](./docs/ARCHITECTURE.md)

# Screenshots

![Home Page](./docs/images/homepage.png)
![Voting Interface](./docs/images/voting.png)
![Leaderboard](./docs/images/leaderboard.png)

# 🎯 Project Overview

MapleTinder brings the fun of ranking and voting to the MapleStory community. Players can browse character profiles, cast their votes, and see which characters reign supreme each week. The project demonstrates full-stack development capabilities with a focus on user engagement, real-time updates, and clean UI/UX design.

**Key Goals:**
- Create an engaging voting experience for the MapleStory community
- Implement fair voting mechanics with anti-manipulation measures
- Build a responsive, modern interface that works across devices
- Demonstrate proficiency in full-stack web development

## ✨ Features

- **Character Voting System** - Browse and vote on MapleStory characters with an intuitive swipe-like interface
- **Weekly Leaderboards** - Real-time rankings that reset weekly to keep content fresh
- **Character Profiles** - Detailed character information including stats, classes, and descriptions
- **Vote History** - Track your voting patterns and favorite characters
- **Responsive Design** - Seamless experience across desktop, tablet, and mobile devices
- **Anti-Manipulation** - IP-based rate limiting and vote validation to ensure fair results
- **Admin Dashboard** - Manage characters, moderate content, and view analytics

## 🛠️ Tech Stack

### Frontend
- Framework: **Vue 3**
- Application Logic: **Javascript** and **Typescript**
- State Management: **Pinia**
- Styling: **Tailwind CSS**
- UI Components: **Shadcn/vue**

### Backend
- Framework: **ASP.NET Core**
- Database: **Microsoft SQL Server**
- ORM: **Entity Framework Core**
- Authentication: **ASP.NET Identity**, **JWT**, **Discord** & **Google OAuth2**
- Testing: **NUnit**

<!-- ### DevOps
- Containerization: Docker & Docker Compose
- CI/CD: GitHub Actions -->

## 🏗️ Architecture

MapleTinder follows a three-tier architecture pattern:

```
┌─────────────────┐      ┌─────────────────┐
│                 │      │                 │      │                 │
│  React Frontend │─────▶│  Express API    │─────▶│   PostgreSQL    │
│                 │      │                 │      │                 │
└─────────────────┘      └─────────────────┘
┌─────────────────┐      ┌─────────────────┐      ┌─────────────────┐
│                 │      │                 │      │                 │
│  React Frontend │─────▶│  Express API    │─────▶│   PostgreSQL    │
│                 │      │                 │      │                 │
└─────────────────┘      └─────────────────┘      └─────────────────┘
                                 │
                                 ▼
                         ┌─────────────────┐
                         │  Redis Cache    │
                         │  (Optional)     │
                         └─────────────────┘
```

See [Architecture Documentation](./docs/ARCHITECTURE.md) for detailed system design.

## 📚 API Documentation

For detailed API documentation, see [API.md](./docs/API.md).

### Quick Reference

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/characters` | Get all characters |
| GET | `/api/characters/:id` | Get character by ID |
| POST | `/api/votes` | Submit a vote |
| GET | `/api/leaderboard` | Get weekly rankings |
| POST | `/api/auth/register` | Register new user |
| POST | `/api/auth/login` | User login |

## 🎨 Design Decisions

### Voting Mechanism
The voting system uses a combination of session tracking and IP-based rate limiting to prevent manipulation while keeping the barrier to entry low for casual users.

### Weekly Reset
Leaderboards reset every Monday at midnight UTC, keeping the competition fresh and encouraging repeat engagement.

### Character Data
Character information is manually curated for quality control, with admin tools for easy updates and additions.

## 🔮 Future Enhancements

- [ ] User authentication system with profiles and vote history
- [ ] Social sharing features (share favorite characters)
- [ ] Character comparison tool
- [ ] Monthly/yearly leaderboards with archive
- [ ] Admin analytics dashboard with voting trends
- [ ] WebSocket integration for real-time leaderboard updates
- [ ] Mobile app version (React Native)
- [ ] Community character submissions with moderation

## 📧 Contact

Elieser Capillar - [GitHub](https://github.com/eliesercapillar) - [LinkedIn](https://www.linkedin.com/in/eliesercapillar/)

Project Link: [https://github.com/eliesercapillar/MapleTinder](https://github.com/eliesercapillar/MapleTinder)

## 🙏 Acknowledgments

- MapleStory character designs and assets are property of Nexon
- Inspiration from Tinder's swipe interface
- MapleStory community for feedback and support

---

⭐️ If you found this project interesting, please consider giving it a star!
