SocialMediaMetrics -- table creation query 

CREATE TABLE SocialMediaMetrics (
    Id INT PRIMARY KEY IDENTITY(1,1),
    MovieName NVARCHAR(255) NOT NULL,
    Platform NVARCHAR(50) NOT NULL,
    TitleOrText NVARCHAR(MAX),
    ViewCount BIGINT DEFAULT 0,
    LikeCount BIGINT DEFAULT 0,
    CommentCount BIGINT DEFAULT 0,
    ShareCount BIGINT DEFAULT 0,
    FetchedAt DATE NOT NULL
);


SocialMediaPages -- table creation query
CREATE TABLE SocialMediaPages (
    Id INT PRIMARY KEY IDENTITY,
    Platform NVARCHAR(100),
    PageUrl NVARCHAR(300),
    PageName NVARCHAR(200),
    FollowersCount BIGINT,
    AdminName NVARCHAR(100),
    AdminMobile NVARCHAR(20)
);
for manaul entry by admin when automation not working
